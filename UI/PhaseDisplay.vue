<template>
    <div v-if="Title != ''" id="PhaseDisplay">
        {{ Title }}
    </div>
</template>

<script>
</script>

<style scoped>
    #PhaseDisplay {
        margin: auto;
        width: 100%;
        padding: 10px;
        text-align: center;
        font-size: 22px;
        color: white;
        text-shadow: 1px 1px 5px rgba( 0, 0, 0, 0.7 );
    }
</style>
