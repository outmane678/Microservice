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
            cleanWs()
        }
        success {
            echo 'Pipeline succeeded - both services deployed to IIS.'
        }
        failure {
            echo 'Pipeline failed - check stage logs for details.'
        }
    }
}
