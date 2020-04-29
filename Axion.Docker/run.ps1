$latestStatus = $null

function Execute-Run {
    Write-Host -NoNewline "`n"
    Write-Host "> Starting dependencies..." -ForegroundColor Blue
    docker-compose up -d

    Write-Host "> Starting main namespace..." -ForegroundColor Blue
    docker run -d --network host --name g_axion gspalato/axion
    Write-Host -NoNewline "`n"
}

function Execute-Build {
    Write-Host -NoNewline "`n"
    Write-Host "> Starting build..." -ForegroundColor Blue
    cd ..
    docker build . -t gspalato/axion
    cd Axion.Docker
    Write-Host -NoNewline "`n"
}

function Prompt() {
    do {
        Write-Host "[Axion Docker Utility]" -ForegroundColor Cyan
        Write-Host "Type 'run' to start, 'build' to create an image." -ForegroundColor Cyan

        #✓✘

        if ($latestStatus -eq $true) {
            Write-Host -NoNewline "$([char]10003) " -ForegroundColor Green
        } elseif ($latestStatus -eq $false) {
            Write-Host -NoNewline "$([char]10008) " -ForegroundColor Red
        }
        $prompt = Read-Host -Prompt "<adu> "

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

Prompt