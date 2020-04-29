$latestStatus = $null

function Execute-Run {
    Write-Host -NoNewline "`n"
    Write-Host "> Starting dependencies..." -ForegroundColor Blue
    docker-compose up -d

    Write-Host "> Starting main namespace..." -ForegroundColor Blue
    docker run -d --network host --name axion gspalato/axion
    Write-Host -NoNewline "`n"
}

function Execute-Build {
    Write-Host -NoNewline "`n"
    Write-Host "> Starting build..." -ForegroundColor Blue
    docker build ../ -t gspalato/axion
    Write-Host -NoNewline "`n"
}

function PromptLoop() {
    do {
        if ($latestStatus -eq $true) {
            Write-Host -NoNewline "$([char]10003) " -ForegroundColor Green
        } elseif ($latestStatus -eq $false) {
            Write-Host -NoNewline "$([char]10008) " -ForegroundColor Red
        }
        Write-Host "<adu> " -NoNewline -ForegroundColor DarkGray
        $prompt = Read-Host

        if ($prompt -like 'run*') {
            $latestStatus = $true
            Try {
                Execute-Run
            } Catch {
                $latestStatus = $false
            }
        }
        elseif ($prompt -like 'build*') {
            $latestStatus = $true
            Try {
                Execute-Build
            } Catch {
                $latestStatus = $false
            }
        }
    } until ($prompt -like 'exit*')

    exit
}

Write-Host "[Axion Docker Utility]" -ForegroundColor Cyan
Write-Host "Type 'run' to start, 'build' to create an image." -ForegroundColor Cyan
PromptLoop