Seguem as instruções para configuração e execução do serviço.

As configurações devem ser ajustadas no arquivo appsettings.json

	DirectoryReceive = Pasta de entrada. Onde os arquivos de origem serão observados e executados ao chegarem.
	DirectoryProcessed = Pasta que o serviço cria os arquivos processados separados, por REG e AVULSO.
	DirectoryArchive = Pasta de backup, onde os arquivos serão movidos após serem executados.
	DirectoryLog = Pasta dos arquivos de Log
	FileExtension = Extensão dos arquivos que devem ser processados (ex: YC).
	ForceCreateAllFiles = Indica se o serviço irá gerar os dois arquivos, mesmo que não haja nenhum avulso de algum dos indicadores true/false. 
						  Caso false, o serviço só irá gerar os arquivos cujo exista retorno bancário.

	IntervalAgent2371Initial = intervalo inicial do identificador avulso 2371 (homologação = 41632370048)
	IntervalAgent2371Final = Intervalo final do identificador avulso 2371 (homologação = 41643140022)

	IntervalAgent237Initial = Intervalo inicial do identificador avulso 237 homologação = 41643140522)
	IntervalAgent237Final = Intervalo final do identificador avulso 2371 (homologação = 99999999999)


======== Instruções de publicação =========

Publicando o Serviço:

Na pasta da soluçao digite:
> dotnet publish -c Release


Windows Service:

Opção de instalar o serviço como Windows Service, usando o utilitário SC do dotnet:
> sc create RoboNewCon binPath=\<pastaDoBinEXE>\RoboArquivosNewcon.exe



Mac OS X

Instalando como Daemon

Crie o script abaixo com nome startup.sh

> vim ~/scripts/startup/startup.sh:

	#!bin/bash
	#Start RoboNewCon if not running
	if [ “$(ps -ef | grep -v grep | grep RoboNewCon | wc -l)” -le 0 ]
	then
	 dotnet /usr/local/services/RoboNewCon.dll
	 echo "RoboNewCon Started"
	else
	 echo "RoboNewCon Already Running"
	fi

Execute o comando abaixo:
> chmod +x ~/scripts/startup/startup.sh

Os daemons neste sistema operacional são definidos na pasta /Library/LaunchDaemons/, onde deve ser configurado em um 
arquivo XML com a extensão .plist (não faz parte do escopo deste artigo explicar as configurações deste arquivo, você 
pode conhecê-las na documentação do launchd).



Linux 

Assim como no Mac OS X, no Linux os serviços são conhecidos como daemon e nesta plataforma também é necessário 
possuir o .NET Core instalado na máquina. Fora isso, cada distribuição pode definir uma forma de criação de daemon, 
aqui vou explicar a criação utilizando o systemd.

No systemd o serviço é definido em um arquivo .service salvo em /etc/systemd/system e que no exemplo deste artigo 
conterá o conteúdo o abaixo:

> vim /etc/systemd/system/robonewcon.service

	[Unit]
	Description=RoboNewCon service

	[Service]  
	ExecStart=/bin/dotnet/RoboNewCon.dll
	WorkingDirectory=/usr/local/services
	User=dotnetuser  
	Group=dotnetuser  
	Restart=on-failure  
	SyslogIdentifier=robonewcon-service  
	PrivateTmp=true  

	[Install]  
	WantedBy=multi-user.target  

Executar o comando abaixo para habilitar o daemon:

> systemctl enable robonewcon.service

Poderá ser iniciado da seguinte forma:

> systemctl start robonewcon.service


