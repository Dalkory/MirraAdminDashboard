import { FormControl, FormLabel } from '@chakra-ui/form-control';
import {
  Box,
  Button,
  Input,
  Heading,
} from '@chakra-ui/react';
import { useToast } from '@chakra-ui/toast';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';

import { useAuth } from '../context/AuthContext';


const Login = () => {
  const [email, setEmail] = useState('admin@mirra.dev');
  const [password, setPassword] = useState('admin123');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const { login } = useAuth();
  const toast = useToast();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      const success = await login(email, password);
      if (success) {
        navigate('/dashboard');
      }
    } catch (err) {
      toast({
        title: 'Error',
        description: 'Invalid email or password',
        status: 'error',
        duration: 5000,
        isClosable: true,
      });
    } finally {
      setLoading(false);
    }
  };

  return (
    <Box 
      minH="100vh" 
      display="flex" 
      alignItems="center" 
      justifyContent="center"
      bg="gray.50"
    >
      <Box 
        bg="white" 
        p={8} 
        borderRadius="md" 
        boxShadow="md" 
        w="full" 
        maxW="md"
      >
        <Heading as="h1" size="lg" mb={6} textAlign="center">
          Admin Dashboard
        </Heading>
        
        <form onSubmit={handleSubmit}>
          <FormControl mb={4}>
            <FormLabel>Email</FormLabel>
            <Input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </FormControl>
          
          <FormControl mb={6}>
            <FormLabel>Password</FormLabel>
            <Input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </FormControl>
          
          <Button
            type="submit"
            colorScheme="blue"
            width="full"
            isLoading={loading}
            _hover={{ bg: 'blue.600' }}
          >
            Login
          </Button>
        </form>
      </Box>
    </Box>
  );
};

export default Login;