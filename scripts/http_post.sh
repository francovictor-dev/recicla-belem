#!/bin/bash

url=http://localhost:5238/api
path=$1
params=$2
token=$3

printf "\n\n$params\n\n"

printf "\nRodando: $url$path...\n\n"

res=$(curl -X POST $url$path \
	-H "Content-Type: application/json" \
	-H "Authorization: Bearer $token" \
	-d "$params")

printf "\n\n$res\n\n"

if [[ $path == "/Auth/login" ]]; then
	export ACCESS_TOKEN=$(echo "$res" | jq -r '.accessToken')
	printf "\n\n$ACCESS_TOKEN\n\n"
fi

printf "\n\nSucesso\n"
