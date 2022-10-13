#!/bin/sh

helm upgrade -i task-service ./task-service --namespace async-course --set image.tag=latest