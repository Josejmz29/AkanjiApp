<template>
    <v-container class="fill-height d-flex justify-center align-center">
        <v-card class="pa-6" width="600">
            <template v-if="!registroExitoso">
                <v-card-title class="text-h5 text-center">Crear cuenta</v-card-title>

                <v-card-text>
                    <!-- Formulario -->
                    <v-form @submit.prevent="handleRegister">
                        <v-text-field v-model="email" label="Correo electrónico" type="email"
                            prepend-inner-icon="mdi-email" required />

                        <v-text-field v-model="fullName" label="Nombre completo" type="text"
                            prepend-inner-icon="mdi-account" required />

                        <v-text-field v-model="tokenZenodo" label="Token Zenodo" type="password"
                            prepend-inner-icon="mdi-lock" required />

                        <v-text-field v-model="password" label="Contraseña" type="password"
                            prepend-inner-icon="mdi-lock" required />

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
            </template>

            <template v-else>
                <v-card-title class="text-h5 text-center">¡Registro exitoso!</v-card-title>
                <v-card-text class="text-center">
                    Serás redirigido al inicio de sesión en unos segundos...
                </v-card-text>
            </template>
        </v-card>
    </v-container>
</template>

<script setup>
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { registrarUsuario } from '@/api/auth.js';

const router = useRouter();

const email = ref('');
const fullName = ref('');
const password = ref('');
const confirmPassword = ref('');
const tokenZenodo = ref('');
const registroExitoso = ref(false);

const handleRegister = async () => {
    if (password.value !== confirmPassword.value) {
        alert('Las contraseñas no coinciden');
        return;
    }

    try {
        const response = await registrarUsuario({
            NombreCompleto: fullName.value,
            Email: email.value,
            Password: password.value,
            ZenodoToken: tokenZenodo.value,
        });

        if (response.status === 200 || response.status === 201 || response.message === "Usuario registrado con éxito") {
            registroExitoso.value = true;

            setTimeout(() => {
                router.push('/login');
            }, 3000); // 3 segundos
        }
    } catch (error) {
        console.error('Error al registrar:', error);
        console.log('Respuesta del backend:', error.response?.data);

        const errores = error?.response?.data?.errors;
        const mensaje = errores?.length ? errores.join('\n') : 'Error desconocido';

        alert(mensaje);
    }
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
