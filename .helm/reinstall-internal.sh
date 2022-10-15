#!/bin/sh

helm uninstall analytics-service --namespace async-course
helm uninstall accounting-service --namespace async-course
helm uninstall task-service --namespace async-course
helm uninstall auth-service --namespace async-course

helm upgrade -i auth-service ./auth-service --namespace async-course --set image.tag=latest
helm upgrade -i task-service ./task-service --namespace async-course --set image.tag=latest
helm upgrade -i accounting-service ./accounting-service --namespace async-course --set image.tag=latest
helm upgrade -i analytics-service ./analytics-service --namespace async-course --set image.tag=latest