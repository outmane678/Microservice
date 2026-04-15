pipeline {
    agent any

    triggers {
        githubPush()
    }

    environment {
        DEPLOY_ROOT  = "C:\\inetpub\\wwwroot"
        DEPT_SERVICE = "DepartementService"
        DEPT_TESTS   = "DepartementService.Tests"
        DEPT_DEPLOY  = "DepartementService"
        EMP_SERVICE  = "EmployeService"
        EMP_TESTS    = "EmployeService.Tests"
        EMP_DEPLOY   = "EmployeServices"
        DEPT_POOL    = "DepartementServicePool"
        EMP_POOL     = "EmployeServicePool"

        DEPT_IMAGE   = "departementservice"
        EMP_IMAGE    = "employeservice"
        DEPT_PORT    = "5001"
        EMP_PORT     = "5002"
    }

    stages {

        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Restore') {
            steps {
                bat 'dotnet restore Microservices.sln'
            }
        }

        stage('Build') {
            parallel {
                stage('Build DepartementService') {
                    steps {
                        bat "dotnet build %DEPT_SERVICE%\\%DEPT_SERVICE%.csproj -c Release --no-restore"
                        bat "dotnet build %DEPT_TESTS%\\%DEPT_TESTS%.csproj -c Release --no-restore"
                    }
                }
                stage('Build EmployeService') {
                    steps {
                        bat "dotnet build %EMP_SERVICE%\\%EMP_SERVICE%.csproj -c Release --no-restore"
                        bat "dotnet build %EMP_TESTS%\\%EMP_TESTS%.csproj -c Release --no-restore"
                    }
                }
            }
        }

        stage('Test') {
            parallel {
                stage('Test DepartementService') {
                    steps {
                        bat "dotnet test %DEPT_TESTS%\\%DEPT_TESTS%.csproj -c Release --no-build --logger trx --results-directory TestResults\\%DEPT_SERVICE%"
                    }
                }
                stage('Test EmployeService') {
                    steps {
                        bat "dotnet test %EMP_TESTS%\\%EMP_TESTS%.csproj -c Release --no-build --logger trx --results-directory TestResults\\%EMP_SERVICE%"
                    }
                }
            }
        }

        // ==================== DOCKER ====================

        stage('Docker Build') {
            parallel {
                stage('Docker Build DepartementService') {
                    steps {
                        bat "docker build -t %DEPT_IMAGE%:latest -f %DEPT_SERVICE%\\Dockerfile ."
                    }
                }
                stage('Docker Build EmployeService') {
                    steps {
                        bat "docker build -t %EMP_IMAGE%:latest -f %EMP_SERVICE%\\Dockerfile ."
                    }
                }
            }
        }

        stage('Docker Run') {
            parallel {
                stage('Run DepartementService Container') {
                    steps {
                        bat 'docker rm -f %DEPT_IMAGE%-test 2>nul || exit /b 0'
                        bat "docker run -d --name %DEPT_IMAGE%-test -p %DEPT_PORT%:8080 %DEPT_IMAGE%:latest"
                    }
                }
                stage('Run EmployeService Container') {
                    steps {
                        bat 'docker rm -f %EMP_IMAGE%-test 2>nul || exit /b 0'
                        bat "docker run -d --name %EMP_IMAGE%-test -p %EMP_PORT%:8080 %EMP_IMAGE%:latest"
                    }
                }
            }
        }

        stage('Docker Health Check') {
            steps {
                bat 'ping -n 6 127.0.0.1 >nul'
                bat "curl -f http://localhost:%DEPT_PORT%/weatherforecast || exit /b 1"
                bat "curl -f http://localhost:%EMP_PORT%/weatherforecast || exit /b 1"
                echo 'Both containers responded successfully.'
            }
        }

        stage('Docker Cleanup') {
            steps {
                bat 'docker stop %DEPT_IMAGE%-test %EMP_IMAGE%-test 2>nul || exit /b 0'
                bat 'docker rm %DEPT_IMAGE%-test %EMP_IMAGE%-test 2>nul || exit /b 0'
            }
        }

        // ==================== IIS DEPLOY ====================

        stage('Publish') {
            parallel {
                stage('Publish DepartementService') {
                    steps {
                        bat "dotnet publish %DEPT_SERVICE%\\%DEPT_SERVICE%.csproj -c Release --no-build -o publish\\%DEPT_SERVICE%"
                    }
                }
                stage('Publish EmployeService') {
                    steps {
                        bat "dotnet publish %EMP_SERVICE%\\%EMP_SERVICE%.csproj -c Release --no-build -o publish\\%EMP_SERVICE%"
                    }
                }
            }
        }

        stage('Stop IIS Pools') {
            parallel {
                stage('Stop DepartementService Pool') {
                    steps {
                        bat 'powershell -Command "$ErrorActionPreference=\'SilentlyContinue\'; Import-Module WebAdministration; Stop-WebAppPool -Name \'%DEPT_POOL%\'; exit 0"'
                        bat 'ping -n 4 127.0.0.1 >nul'
                    }
                }
                stage('Stop EmployeService Pool') {
                    steps {
                        bat 'powershell -Command "$ErrorActionPreference=\'SilentlyContinue\'; Import-Module WebAdministration; Stop-WebAppPool -Name \'%EMP_POOL%\'; exit 0"'
                        bat 'ping -n 4 127.0.0.1 >nul'
                    }
                }
            }
        }

        stage('Deploy Files') {
            parallel {
                stage('Deploy DepartementService') {
                    steps {
                        bat """
                            robocopy publish\\%DEPT_SERVICE% %DEPLOY_ROOT%\\%DEPT_DEPLOY% /MIR /R:3 /W:5
                            if %ERRORLEVEL% LEQ 7 exit /b 0
                        """
                    }
                }
                stage('Deploy EmployeService') {
                    steps {
                        bat """
                            robocopy publish\\%EMP_SERVICE% %DEPLOY_ROOT%\\%EMP_DEPLOY% /MIR /R:3 /W:5
                            if %ERRORLEVEL% LEQ 7 exit /b 0
                        """
                    }
                }
            }
        }

        stage('Start IIS Pools') {
            parallel {
                stage('Start DepartementService Pool') {
                    steps {
                        bat 'powershell -Command "$ErrorActionPreference=\'SilentlyContinue\'; Import-Module WebAdministration; if (-not (Test-Path IIS:\\AppPools\\%DEPT_POOL%)) { New-WebAppPool \'%DEPT_POOL%\'; Set-ItemProperty IIS:\\AppPools\\%DEPT_POOL% managedRuntimeVersion \'\' }; Start-WebAppPool \'%DEPT_POOL%\'; exit 0"'
                    }
                }
                stage('Start EmployeService Pool') {
                    steps {
                        bat 'powershell -Command "$ErrorActionPreference=\'SilentlyContinue\'; Import-Module WebAdministration; if (-not (Test-Path IIS:\\AppPools\\%EMP_POOL%)) { New-WebAppPool \'%EMP_POOL%\'; Set-ItemProperty IIS:\\AppPools\\%EMP_POOL% managedRuntimeVersion \'\' }; Start-WebAppPool \'%EMP_POOL%\'; exit 0"'
                    }
                }
            }
        }
    }

    post {
        always {
            bat 'docker stop %DEPT_IMAGE%-test %EMP_IMAGE%-test 2>nul || exit /b 0'
            bat 'docker rm %DEPT_IMAGE%-test %EMP_IMAGE%-test 2>nul || exit /b 0'
            cleanWs()
        }
        success {
            echo 'Pipeline succeeded - Docker tested + both services deployed to IIS.'
        }
        failure {
            echo 'Pipeline failed - check stage logs for details.'
        }
    }
}
