function Remove-All-Containers {
	docker container stop $(docker ps -aq)
	docker container rm $(docker ps -aq)
}

function Show-Help {
	Write-Host ""
	Write-Host "Axion.Docker" -ForegroundColor blue
	Write-Host ""
	Write-Host "Usage" -ForegroundColor yellow
	Write-Host "
./docker/docker.sh [COMMAND] [ARGS...]
./docker/docker.sh -h | --help"
	Write-Host ""
	Write-Host "Commands" -ForegroundColor yellow
	Write-Host "
build		Builds a Docker image so it is prepped for running"
}

function Step-Run {
	Param (
		[String]$command = $( Read-Host "What command do you want to run? If unsure type help" ),
		[string]$Service
	)

	Begin {
		if ($command -Ne "help" -And $command -Ne "h" -And $command -Ne "removeall" -And $service -Eq "") {
			$service = $( Read-Host "What Docker service do you want to control?" )
		}
	}

	Process {
		switch ( $command ) {
			build { docker build . }
			default { Show-Help }
		}
	}
}

Step-Run