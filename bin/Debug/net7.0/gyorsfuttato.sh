#!/bin/bash

A=$(./titkosito -e "hello world" "keykeykeykeykey")
./titkosito -d $A "keykeykeykeykey"

./titkosito -d "cvtlsxo fiutxysspjzxtxwp" "abcdefghijklmnopqrstuvwxyzabcdefg"
./titkosito -d "ebtobehpzmjnmfqwuirlazvslpm" "abcdefghijklmnopqrstuvwxyzabcdefg"


./titkosito -c "cvtlsxo fiutxysspjzxtxwp" "ebtobehpzmjnmfqwuirlazvslpm" > output.txt
./titkosito -c "ebtobehpzmjnmfqwuirlazvslpm" "cvtlsxo fiutxysspjzxtxwp" > outputalt.txt