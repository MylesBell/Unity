#!/bin/bash

echo "Type (host|client)"
read type
echo "IP Address:"
read ip
port=51091
echo "Number of screens:"
read nos
echo "Screen Number:"
read sn

./game.x86_64 --type=$type --ip=$ip --port=$port --screen-number=$sn --number-of-screens=$nos