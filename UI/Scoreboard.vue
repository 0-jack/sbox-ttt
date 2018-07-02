
<template>

    <div id="Scoreboard" v-bind:class="{ hidden: !Visible }">
        <div class="inner">

            <h2>{{ServerName}}</h2>

            <table>
                <thead>
                    <tr>
                        <td style="padding-left: 64px;">&nbsp;</td>
                        <td width="90%">&nbsp;</td>
                        <td width="100px" style="text-align: right">Kills</td>
                        <td width="100px" style="text-align: right">Deaths</td>
                        <td width="100px" style="text-align: right">Score</td>
                        <td width="100px" style="text-align: right; padding-right: 64px;">Ping</td>
                    </tr>
                </thead>
                <tbody>

                    <tr v-for="player in orderBy( PlayerList, 'Score', 'desc' )" :class="{ LocalPlayer: player.IsLocalPlayer }">
                        <td style="padding-left: 64px;">
                            <menusystem-steamavatar :steamid="player.UserId" size="42"></menusystem-steamavatar>
                        </td>
                        <td>{{player.Username}}</td>
                        <td style="text-align: right">{{player.Kills}}</td>
                        <td style="text-align: right">{{player.Deaths}}</td>
                        <td style="text-align: right">{{player.Score}}</td>
                        <td style="text-align: right; padding-right: 64px;">{{player.Ping}}</td>
                    </tr>

                </tbody>
            </table>

        </div>
    </div>

</template>

<style>
    #Scoreboard {
        position: absolute;
        left: 0;
        right: 0px;
        top: 0;
        bottom: 0;
        text-align: center;
        background-color: rgba( 0, 0, 0, 0.2 );
        display: flex;
        align-items: center;
        justify-content: center;
        pointer-events: none;
        touch-action: none;
        transition: all 0.05s ease-out;
        transform: scale( 1.0, 1.0 );
    }

        #Scoreboard.hidden {
            opacity: 0;
            transition: all 0.1s ease-out 0s;
            transform: scale( 1.1, 1.1 );
        }

        #Scoreboard .inner {
            background-color: rgba( 0, 0, 0, 0.3 );
            width: 800px;
            height: 600px;
            padding: 5px 10px;
            border: 1px solid #fa0;
            border-radius: 5px;
            overflow: hidden;
        }

            #Scoreboard .inner h2 {
                text-align: left;
                color: #fa0;
                padding: 16px;
                margin: 0;
            }

            #Scoreboard .inner table {
                padding: 32px;
                width: 100%;
                color: rgba(236, 239, 241,1.0);
                padding: 0;
                border: 0;
                border-collapse: collapse;
            }

                #Scoreboard .inner table tbody td {
                    padding: 3px;
                    font-size: 13px;
                }

                #Scoreboard .inner table thead td {
                    color: #fa0;
                    font-weight: bolder !important;
                    padding: 10px;
                }

                #Scoreboard .inner table tbody tr {
                    background-color: rgba(96, 125, 139, 0.1);
                }

                    #Scoreboard .inner table tbody tr:nth-child(odd) {
                        background-color: rgba(96, 125, 139, 0.05);
                    }

                    #Scoreboard .inner table tbody tr.LocalPlayer {
                        font-weight: bolder !important;
                        background-color: rgba(255, 200, 0, 0.2);
                    }
</style>