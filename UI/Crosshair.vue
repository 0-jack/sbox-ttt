
<template>
    <div id="crosshair" v-if="Visible">
        <div class="inner">
            <div class="top"></div>
            <div class="bottom"></div>
            <div class="left"></div>
            <div class="right"></div>
        </div>
    </div>
</template>

<script>

    module.exports =
        {
            Punch: function (x) {
                $("#crosshair .inner").velocity("stop")
                    .velocity({ width: 10 + x * 4, height: 10 + x * 4 }, { duration: 20, ease: 'easeout' })
                    .velocity({ width: 10, height: 10 }, { duration: 200, ease: 'easein' });
            }
        }

</script>

<style lang="less">
    #crosshair {
        position: absolute;
        top: 0px;
        right: 0px;
        left: 0px;
        bottom: 0px;
        display: flex;
        align-items: center;
        justify-content: center;
        pointer-events: none;

        >div
        {
            width: 10px;
            height: 10px;
            position: relative;

            .left, .right, .top, .bottom
            {
                background-color: #fa0;
                width: 2px;
                height: 2px;
                position: absolute;
                left: 50%;
                right: 50%;
                top: 50%;
                bottom: 50%;
            }

            .left, .right {
                width: 5px;
            }

            .top, .bottom {
                height: 5px;
            }

            .left {
                left: auto;
                left: -2px;
            }

            .right {
                left: auto;
                right: -4px !important;
            }

            .top {
                top: -2px;
            }

            .bottom {
                bottom: -4px;
                top: auto;
            }
        }
    }
</style>