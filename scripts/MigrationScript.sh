#!/bin/bash

path_value=

while getopts "n:" opt; do
	case $opt in
		n)
			path_value=$OPTARG
			;;
		\?)
			exit 1
			;;
	esac
done

if [ -z "$path_value" ]; then
	echo "Adicione o valor do path usando parametro -n"
	exit 1
fi

echo "Iniciando migration..."

dotnet ef migrations add $path_value
dotnet ef database update

echo "Migration com sucesso!"
