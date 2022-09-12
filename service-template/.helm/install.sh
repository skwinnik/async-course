#!/bin/sh

helm upgrade -i service-template ./service-template --namespace async-course --set image.tag=latest