#!/bin/sh

helm upgrade -i auth-service ./auth-service --namespace async-course --set image.tag=latest