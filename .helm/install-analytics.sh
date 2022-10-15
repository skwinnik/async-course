#!/bin/sh

helm upgrade -i analytics-service ./analytics-service --namespace async-course --set image.tag=latest