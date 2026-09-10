#!/bin/bash

url=http://localhost:5238/api
path=$1
token=$ACCESS_TOKEN

printf "Rodando: $url$path...\n\n"

curl $url$path -H "Authorization: Bearer $token"

printf "\n\nSucesso!\n"

