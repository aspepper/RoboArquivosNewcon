using System;
using System.Globalization;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace RoboArquivosNewcon
{

    public class ProcessaCNAB
    {

        private readonly string _fileCNAB400 = string.Empty;
        private readonly IConfiguration _configuration;

        public ProcessaCNAB(string filename, IConfiguration configuration)
        {
            _fileCNAB400 = filename;
            _configuration = configuration;
        }

        public bool ProcessCNAB400()
        {

            string folderLog = _configuration.GetSection("DirectoryLog").Value;
            string folderDestination = _configuration.GetSection("DirectoryProcessed").Value;
            string folderBackup = _configuration.GetSection("DirectoryArchive").Value;
            string fileLogName = Path.Combine(folderLog, string.Format("{0}.log", DateTime.Now.ToString("yyyyMMddHHmm")));

            if (File.Exists(Path.Combine(folderBackup, Path.GetFileName(_fileCNAB400))))
            {
                return false;
            }

            FileStream fileLog;
            if (!File.Exists(fileLogName))
            { fileLog = new FileStream(fileLogName, FileMode.CreateNew); }
            else
            { fileLog = new FileStream(fileLogName, FileMode.Append); }

            double totalAvulso237 = 0;
            double totalAvulso2371 = 0;
            bool ForceCreateAllFiles = bool.Parse(_configuration.GetSection("ForceCreateAllFiles").Value);

            long intervalAgent2371Initial = long.Parse(_configuration.GetSection("IntervalAgent2371Initial").Value);
            long intervalAgent2371Final = long.Parse(_configuration.GetSection("IntervalAgent2371Final").Value);
            long intervalAgent237Initial = long.Parse(_configuration.GetSection("IntervalAgent237Initial").Value);
            long intervalAgent237Final = long.Parse(_configuration.GetSection("IntervalAgent237Final").Value);

            using (StreamWriter flog = new StreamWriter(fileLog))
            {
                int currLine = 0;
                string line = string.Empty;

                try
                {

                    DirectoryInfo di;
                    if (!Directory.Exists(folderDestination)) { di = Directory.CreateDirectory(folderDestination); }
                    if (!Directory.Exists(folderLog)) { di = Directory.CreateDirectory(folderLog); }
                    if (!Directory.Exists(folderBackup)) { di = Directory.CreateDirectory(folderBackup); }

                    if (File.Exists(Path.Combine(folderDestination, string.Format("BRDSAVULSO{0}.YC", DateTime.Now.ToString("ddMM")))))
                    { File.Delete(Path.Combine(folderDestination, string.Format("BRDSAVULSO{0}.YC", DateTime.Now.ToString("ddMM")))); }
                    if (File.Exists(Path.Combine(folderDestination, string.Format("BRDSREG{0}.YC", DateTime.Now.ToString("ddMM")))))
                    { File.Delete(Path.Combine(folderDestination, string.Format("BRDSREG{0}.YC", DateTime.Now.ToString("ddMM")))); }

                    FileStream fileSource = new FileStream(_fileCNAB400, FileMode.Open);
                    FileStream fileAvulso237 = new FileStream(Path.Combine(folderDestination, string.Format("BRDSAVULSO{0}.YC", DateTime.Now.ToString("ddMM"))), FileMode.CreateNew);
                    FileStream fileAvulso2371 = new FileStream(Path.Combine(folderDestination, string.Format("BRDSREG{0}.YC", DateTime.Now.ToString("ddMM"))), FileMode.CreateNew);

                    double valor;

                    flog.WriteLine("{0} - Iniciando o processamento do arquivo {1}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), Path.GetFileName(_fileCNAB400));
                    using StreamReader freader = new StreamReader(fileSource);
                    using StreamWriter fwriter2371 = new StreamWriter(fileAvulso2371);
                    using StreamWriter fwriter237 = new StreamWriter(fileAvulso237);

                    while ((line = freader.ReadLine()) != null)
                    {

                        currLine++;

                        if (line[0] == '1')
                        {
                            if (!long.TryParse(line.Substring(70, 11), out long identificador)) { identificador = 0; }

                            // 2371
                            if ((identificador.CompareTo(intervalAgent2371Initial) >= 0) && (identificador.CompareTo(intervalAgent2371Final) <= 0))
                            {
                                fwriter2371.WriteLine(line);
                                // Vetifica se tem data de pagto
                                if (!string.IsNullOrWhiteSpace(line.Substring(295, 9)))
                                {
                                    if (!double.TryParse(line.Substring(152, 13), out valor)) { valor = 0; }
                                    totalAvulso2371 += (valor / 100);
                                }
                            }
                            // 237
                            else
                            if ((identificador.CompareTo(intervalAgent237Initial) >= 0) && (identificador.CompareTo(intervalAgent237Final) <= 0))
                            {
                                fwriter237.WriteLine(line);
                                // Vetifica se tem data de pagto
                                if (!string.IsNullOrWhiteSpace(line.Substring(295, 9)))
                                {
                                    if (!double.TryParse(line.Substring(152, 13), out valor)) { valor = 0; }
                                    totalAvulso237 += (valor / 100);
                                }
                            }

                        }
                        else
                        {
                            fwriter2371.WriteLine(line);
                            fwriter237.WriteLine(line);
                        }
                    }

                    flog.WriteLine("{0} - Processamento Finalizado com Sucesso", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                    flog.WriteLine("{0} - Total Avulso 2371 = {1}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), totalAvulso2371.ToString("#,###,##0.00", CultureInfo.CreateSpecificCulture("pt-BR")));
                    flog.WriteLine("{0} - Total Avulso 237  = {1}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), totalAvulso237.ToString("#,###,##0.00", CultureInfo.CreateSpecificCulture("pt-BR")));

                }
                catch (Exception ex)
                {
                    flog.WriteLine("{0} - Erro no processamento do arquivo na linha {1}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), currLine);
                    flog.WriteLine("      Line error : {0}", line);
                    flog.WriteLine("      Error message: {0}", ex.Message);
                    flog.WriteLine(ex.StackTrace);
                    return false;
                }

                if (!ForceCreateAllFiles)
                {
                    if (totalAvulso237.Equals(0)) { File.Delete(Path.Combine(folderDestination, string.Format("BRDSAVULSO{0}.YC", DateTime.Now.ToString("ddMM")))); }
                    if (totalAvulso2371.Equals(0)) { File.Delete(Path.Combine(folderDestination, string.Format("BRDSREG{0}.YC", DateTime.Now.ToString("ddMM")))); }
                }

            }

            File.Copy(_fileCNAB400, Path.Combine(folderBackup, Path.GetFileName(_fileCNAB400)), true);
            File.Delete(_fileCNAB400);
            return true;
        }
    }
}
