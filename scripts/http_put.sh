#!/bin/bash

url=http://localhost:5238/api
path=$1
params=$2
token=$ACCESS_TOKEN

printf "\nRodando: $url$path...\n\n"

curl -X PUT $url$path \
	-H "Content-Type: application/json" \
	-H "Authorization: Bearer $token" \
	-d "$params"

printf "\n\nSucesso\n"
