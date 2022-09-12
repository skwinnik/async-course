#!/bin/sh

helm upgrade -i rabbitmq bitnami/rabbitmq -f rabbitmq/values.yaml --namespace async-course
helm upgrade -i postgresql bitnami/postgresql --namespace async-course
helm upgrade -i kafka bitnami/kafka --namespace async-course