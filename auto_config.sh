#!/bin/bash
# Automatic HeadgearsOfWar Game Configuration script

# tar -zcvf ./game-sync.tar.gz ./Automation
# scp -r ./game-sync.tar.gz jf13282@snowy.cs.bris.ac.uk:~/
# ssh jf13282@snowy.cs.bris.ac.uk "tar -zxvf ~/game-sync.tar.gz"

SERVER="it025968"
GAME_CODE="abcd"

declare -a RIGHT_CLIENTS=("it025968" "it025969" "it025970" "it025971" "it025972")
declare -a LEFT_CLIENTS=("it025985" "it025984" "it025983" "it025982" "it025981")

SCREENS_LEFT=${#LEFT_CLIENTS[@]}
SCREENS_RIGHT=${#RIGHT_CLIENTS[@]}

# Terminate all the RHS and LHS clients
for client in "${RIGHT_CLIENTS[@]}"
do
   ssh jf13282@$client.users.bris.ac.uk "pkill -f LinuxGame.x86_64 &" &
done

for client in "${LEFT_CLIENTS[@]}"
do
   ssh jf13282@$client.users.bris.ac.uk "pkill -f LinuxGame.x86_64 &" &
done

echo "Active games killed, press any key to launch server"
read -n 1 -s

# SSH to the chosen server and configure and run the game
ssh jf13282@$SERVER.users.bris.ac.uk "DISPLAY=:0 nohup ~/LinuxGame.x86_64 --type=host --ip=localhost --port=51091 --number-of-screens-left=$SCREENS_LEFT --number-of-screens-right=$SCREENS_RIGHT --game-code=$GAME_CODE --use-path-finding=true --show-path-finding=false -screen-height 1080 -screen-width 1920 -screen-fullscreen 1 -screen-quality Fastest </dev/null >myprogram.log 2>&1 &" &
echo "Server launched, press any key to launch clients"
read -n 1 -s

# SSH to the RHS clietns in order and configure and run the game
screen_count="0"
for client in "${RIGHT_CLIENTS[@]}"
do
   ssh jf13282@$client.users.bris.ac.uk "DISPLAY=:0 nohup ~/LinuxGame.x86_64 --type=client --ip=$SERVER.users.bris.ac.uk --port=51091 --number-of-screens-left=$SCREENS_LEFT --number-of-screens-right=$SCREENS_RIGHT --screen-number=$screen_count --lane=right -screen-height 1080 -screen-width 1920 -screen-fullscreen 1 -screen-quality Fantastic </dev/null >myprogram.log 2>&1 &" &
   screen_count=$((screen_count+1))
done

# SSH to the LHS clients in order and configure and run the game
screen_count="0"
for client in "${LEFT_CLIENTS[@]}"
do
   ssh jf13282@$client.users.bris.ac.uk "DISPLAY=:0 nohup ~/LinuxGame.x86_64 --type=client --ip=$SERVER.users.bris.ac.uk --port=51091 --number-of-screens-left=$SCREENS_LEFT --number-of-screens-right=$SCREENS_RIGHT --screen-number=$screen_count --lane=left -screen-height 1080 -screen-width 1920 -screen-fullscreen 1 -screen-quality Fantastic </dev/null >myprogram.log 2>&1 &" &
   screen_count=$((screen_count+1))
done
