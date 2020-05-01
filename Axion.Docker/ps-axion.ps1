$latestStatus = $null

function Execute-Run {
    Write-Host -NoNewline "`n"
    Write-Host "> Starting dependencies..." -ForegroundColor Blue
    docker-compose up -d

    Write-Host "> Starting main namespace..." -ForegroundColor Blue
    docker run -d --network host --name axion gspalato/axion
    Write-Host -NoNewline "`n"
}

function Execute-Stop {
    Write-Host -NoNewline "`n"
    Write-Host "> Stopping services.." -ForegroundColor Blue
    docker stop axion axion_database_1 axion_lavalink_1
    docker rm axion axion_database_1 axion_lavalink_1

    Write-Host -NoNewline "`n"
}

function Execute-Build {
    Write-Host -NoNewline "`n"
    Write-Host "> Starting build..." -ForegroundColor Blue
    docker build ../ -t gspalato/axion
    Write-Host -NoNewline "`n"
}

function Execute-Exec {
    Write-Host -NoNewline "`n"
    Write-Host "> Attaching bash..." -ForegroundColor Blue
    docker exec -it axion bash
    Write-Host -NoNewline "`n"
}

function RunCommand($prompt) {
    if ($prompt -like 'run*') {
        $latestStatus = $true
        Try { Execute-Run } Catch { $latestStatus = $false }
    }
    elseif ($prompt -like 'build*') {
        $latestStatus = $true
        Try { Execute-Build } Catch { $latestStatus = $false }
    }
    elseif ($prompt -like 'bash*') {
        $latestStatus = $true
        Try { Execute-Exec } Catch { $latestStatus = $false }
    }
    elseif ($prompt -like 'stop*') {
        $latestStatus = $true
        Try { Execute-Stop } Catch { $latestStatus = $false }
    }
}

function PromptLoop($prompt) {
    do {
        if ($latestStatus -eq $true) {
            Write-Host -NoNewline "$([char]10003) " -ForegroundColor Green
        } elseif ($latestStatus -eq $false) {
            Write-Host -NoNewline "$([char]10008) " -ForegroundColor Red
        }
        Write-Host "<adu> " -NoNewline -ForegroundColor Gray
        $prompt = Read-Host

        RunCommand $prompt
    } until ($prompt -like 'exit*')

    exit
}

if ($Args[0] -ne $null) {
    RunCommand $Args[0]
}

Write-Host "[Axion Docker Utility]" -ForegroundColor Cyan
Write-Host "Type 'run' to start, 'build' to create an image." -ForegroundColor Cyan
PromptLoop