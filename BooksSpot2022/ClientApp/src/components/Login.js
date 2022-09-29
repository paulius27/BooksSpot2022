import React from 'react';
import { useNavigate } from 'react-router-dom';

const Login = () => {
    const navigate = useNavigate();

    const signIn = () => {
        const username = document.getElementById('username').value;
        const password = document.getElementById('password').value;

        const requestOptions = {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({username, password})
        };

        fetch("/authenticate/login", requestOptions)
            .then(res => res.json())
            .then(
                (result) => {
                    console.log('Login:', result);

                    if ('token' in result)
                    {
                        localStorage.setItem('token', result.token);
                        navigate('/');
                    }
                    else
                    {
                        alert('Login failed.');
                    }
                },
                (error) => {
                    alert('Login failed.');
                    console.log('Error:', error);
                }
            );
    }

    return (
        <div>
            <h1>Login</h1>

            <form>
                <div className="form-outline mb-4">
                    <input type="text" id="username" className="form-control" />
                    <label className="form-label" htmlFor="username">Username</label>
                </div>

                <div className="form-outline mb-4">
                    <input type="password" id="password" className="form-control" />
                    <label className="form-label" htmlFor="password">Password</label>
                </div>

                <button type="button" className="btn btn-primary btn-block mb-4" onClick={signIn}>Sign in</button>
            </form>
        </div>
    );
    
}

export default Login;