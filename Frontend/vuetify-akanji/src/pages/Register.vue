<template>
    <v-container class="fill-height d-flex justify-center align-center">
        <v-card class="pa-6" width="600">
            <v-card-title class="text-h5 text-center">Crear cuenta</v-card-title>

            <v-card-text>
                <!-- Formulario -->
                <v-form @submit.prevent="handleRegister">
                    <v-text-field v-model="email" label="Correo electrónico" type="email" prepend-inner-icon="mdi-email"
                        required />

                    <v-text-field v-model="fullName" label="Nombre completo" type="text"
                        prepend-inner-icon="mdi-account" required />

                    <v-text-field v-model="password" label="Contraseña" type="password" prepend-inner-icon="mdi-lock"
                        required />

                    <v-text-field v-model="confirmPassword" label="Confirmar contraseña" type="password"
                        prepend-inner-icon="mdi-lock-check" required />

                    <v-btn color="primary" block class="mt-2" type="submit">
                        Registrarse
                    </v-btn>
                </v-form>

                <!-- Enlace para volver al login -->
                <div class="text-center mt-6">
                    ¿Ya tienes una cuenta?
                    <span class="text-primary cursor-pointer" @click="goToLogin">
                        Inicia sesión
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
const fullName = ref('');
const password = ref('');
const confirmPassword = ref('');

const handleRegister = () => {
    if (password.value !== confirmPassword.value) {
        alert('Las contraseñas no coinciden');
        return;
    }
    console.log('Registrando usuario:', {
        email: email.value,
        fullName: fullName.value,
        password: password.value,
    });
};

const goToLogin = () => {
    router.push('/login');
};
</script>

<style scoped>
.cursor-pointer {
    cursor: pointer;
}
</style>