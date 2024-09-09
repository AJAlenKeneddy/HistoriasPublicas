// wwwroot/js/auth.js
function saveToken(token) {
    localStorage.setItem('authToken', token);
}

async function checkAuthToken() {
    const token = localStorage.getItem('authToken');

    if (!token) {
        console.log('No token found');
        return false;
    }

    try {
        const response = await fetch('https://localhost:7184/api/Acceso/VerifyToken', {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (response.ok) {
            console.log('Token is valid');
            return true;
        } else {
            console.log('Token is invalid');
            localStorage.removeItem('authToken');
            return false;
        }
    } catch (error) {
        console.error('Error verifying token:', error);
        return false;
    }
}

document.addEventListener('DOMContentLoaded', (event) => {
    checkAuthToken().then(isAuthenticated => {
        if (!isAuthenticated) {
            
            window.location.href = '/login';
        } else {
            
            
        }
    });
});
