#!/bin/bash

A=$(./titkosito -e "hello world" "keykeykeykeykey")
./titkosito -d $A "keykeykeykeykey"

./titkosito -d "cvtlsxo fiutxysspjzxtxwp" "abcdefghijklmnopqrstuvwxyzabcdefg"
./titkosito -d "ebtobehpzmjnmfqwuirlazvslpm" "abcdefghijklmnopqrstuvwxyzabcdefg"


./titkosito -c "cvtlsxo fiutxysspjzxtxwp" "ebtobehpzmjnmfqwuirlazvslpm" "teszt" > output.txt
./titkosito -c "ebtobehpzmjnmfqwuirlazvslpm" "cvtlsxo fiutxysspjzxtxwp" "teszt" > outputalt.txt
./titkosito -c "cvtlsxo fiutxysspjzxtxwp" "ebtobehpzmjnmfqwuirlazvslpm"  > outputLive.txt
./titkosito -c "ebtobehpzmjnmfqwuirlazvslpm" "cvtlsxo fiutxysspjzxtxwp"  > outputaltLive.txt