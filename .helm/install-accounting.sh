#!/bin/sh

helm upgrade -i accounting-service ./accounting-service --namespace async-course --set image.tag=latest