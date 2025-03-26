<template>
    <v-container class="fill-height d-flex justify-center align-center">
        <v-card class="pa-6" width="650">
            <v-card-title class="text-h5 text-center">Iniciar sesión</v-card-title>

            <v-card-text>
                <!-- Formulario -->
                <v-form @submit.prevent="handleLogin">
                    <v-text-field v-model="email" label="Correo electrónico" type="email" prepend-inner-icon="mdi-email"
                        required />

                    <v-text-field v-model="password" label="Contraseña" type="password" prepend-inner-icon="mdi-lock"
                        required />

                    <v-btn color="primary" block class="mt-2" type="submit">
                        Login
                    </v-btn>
                </v-form>

                <!-- Botón Iniciar sesión con GitHub -->
                <v-btn color="black" block class="mt-4" @click="loginWithGithub">
                    <v-icon left>mdi-github</v-icon>
                    Iniciar sesión con GitHub
                </v-btn>

                <!-- Enlace a Registro -->
                <div class="text-center mt-6">
                    ¿No tienes cuenta?
                    <span class="text-primary cursor-pointer" @click="goToRegister">
                        Regístrate ahora
                    </span>
                </div>
            </v-card-text>
        </v-card>
    </v-container>
</template>

<script setup>
import { ref } from 'vue';
import { useRouter } from 'vue-router';

const router = useRouter();
const email = ref('');
const password = ref('');

const handleLogin = () => {
    console.log('Iniciando sesión con:', email.value, password.value);
    // Aquí iría la lógica de autenticación con API o Firebase
};

const loginWithGithub = () => {
    console.log('Iniciar sesión con GitHub');
    // Aquí iría la lógica de autenticación con GitHub
};

const goToRegister = () => {
    router.push('/register'); // Redirige al registro
};
</script>

<style scoped>
.cursor-pointer {
    cursor: pointer;
}
</style>