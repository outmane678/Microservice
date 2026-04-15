pipeline {
    agent any

    triggers {
        githubPush()
    }

    environment {
        DEPT_SERVICE = "DepartementService"
        DEPT_TESTS   = "DepartementService.Tests"
        EMP_SERVICE  = "EmployeService"
        EMP_TESTS    = "EmployeService.Tests"
        DEPT_IMAGE   = "departementservice"
        EMP_IMAGE    = "employeservice"
    }

    stages {

        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        // ==================== BUILD & TEST .NET ====================

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

        // ==================== DOCKER BUILD ====================

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

        // ==================== DOCKER TEST ====================

        stage('Docker Test') {
            steps {
                bat 'docker rm -f %DEPT_IMAGE%-test %EMP_IMAGE%-test 2>nul || exit /b 0'
                bat "docker run -d --name %DEPT_IMAGE%-test -p 6001:8080 %DEPT_IMAGE%:latest"
                bat "docker run -d --name %EMP_IMAGE%-test -p 6002:8080 %EMP_IMAGE%:latest"
                bat 'ping -n 6 127.0.0.1 >nul'
                bat 'curl -f http://localhost:6001/weatherforecast || exit /b 1'
                bat 'curl -f http://localhost:6002/weatherforecast || exit /b 1'
                echo 'Both containers responded HTTP 200.'
                bat 'docker stop %DEPT_IMAGE%-test %EMP_IMAGE%-test 2>nul || exit /b 0'
                bat 'docker rm %DEPT_IMAGE%-test %EMP_IMAGE%-test 2>nul || exit /b 0'
            }
        }

        // ==================== DOCKER DEPLOY ====================

        stage('Deploy with Docker Compose') {
            steps {
                bat 'docker-compose down --remove-orphans 2>nul || exit /b 0'
                bat 'docker rm -f departementservice employeservice nginx-proxy 2>nul || exit /b 0'
                bat 'docker-compose up -d'
                bat 'ping -n 6 127.0.0.1 >nul'
                bat 'curl -f http://localhost:9090/DepartementService/weatherforecast || exit /b 1'
                bat 'curl -f http://localhost:9090/EmployeService/weatherforecast || exit /b 1'
                echo 'Docker Compose deployment verified - all services running behind Nginx.'
            }
        }
    }

    post {
        always {
            bat 'docker stop %DEPT_IMAGE%-test %EMP_IMAGE%-test 2>nul || exit /b 0'
            bat 'docker rm %DEPT_IMAGE%-test %EMP_IMAGE%-test 2>nul || exit /b 0'
        }
        success {
            echo 'Pipeline succeeded - services deployed with Docker Compose.'
        }
        failure {
            echo 'Pipeline failed - check stage logs for details.'
        }
    }
}
