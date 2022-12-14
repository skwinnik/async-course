#!/bin/sh

docker build -t skwinnik/auth-service -f AuthService/Dockerfile .
docker build -t skwinnik/task-service -f TaskService/Dockerfile .
docker build -t skwinnik/accounting-service -f AccountingService/Dockerfile .
docker build -t skwinnik/analytics-service -f AnalyticsService/Dockerfile .

docker push skwinnik/auth-service
docker push skwinnik/task-service
docker push skwinnik/accounting-service
docker push skwinnik/analytics-service
