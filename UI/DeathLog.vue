
<template>

    <div id="DeathLog">

        <transition-group appear name="fade" tag="div">

            <div v-for="entry in Entries" class="Message" v-bind:key="entry.Id">

                <span :style="{color: entry.AttackerColor}" class="attacker">{{entry.Attacker}}</span>
                <img src="/UI/Images/bomb3.png" />
                <span :style="{color: entry.VictimColor}" class="attacker">{{entry.Victim}}</span>

            </div>

        </transition-group>

    </div>

</template>

<style scoped>
    #DeathLog {
        position: absolute;
        right: 32px;
        top: 64px;
    }

        #DeathLog > div {
            display: flex;
            flex-direction: column-reverse;
        }

        #DeathLog .Message {
            position: relative;
            border-radius: 5px;
            font-size: 14px;
            font-weight: bolder;
            background-color: rgba( 30, 30, 30, 0.4 );
            color: #fff;
            margin-bottom: 2px;
            height: 36px;
            overflow: hidden;
            text-shadow: 1px 1px 5px rgba( 0, 0, 0, 0.7 );
        }

            #DeathLog .Message span {
                padding: 10px 20px;
                display: inline-block;
                overflow: hidden;
                white-space: nowrap;
            }

        #DeathLog .fade-enter-active, #DeathLog .fade-leave-active {
            transition: all 0.4s cubic-bezier(.75,-0.5,0,1.75);
        }

        #DeathLog .fade-leave-to {
            opacity: 0;
            transform: translateX(200px);
        }

        #DeathLog .fade-enter {
            opacity: 0;
            transform: translateX(200px);
            height: 0px;
        }
</style>
