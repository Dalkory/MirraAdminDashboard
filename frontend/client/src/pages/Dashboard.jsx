import { FormControl, FormLabel } from '@chakra-ui/form-control';
import { 
  Box, 
  Flex, 
  Heading, 
  Button, 
  Grid, 
  GridItem, 
  Input,
  Stack,
  Text,
  Spinner,

} from '@chakra-ui/react';
import { useToast } from '@chakra-ui/toast';
import { useState, useEffect } from 'react';

import { getClients, createClient } from '../api/clients';
import { getPayments } from '../api/payments';
import { getRate } from '../api/rate';
import { getTags } from '../api/tags';
import ClientTable from '../components/ClientTable';
import RateCard from '../components/RateCard';
import TagManager from '../components/TagManager';
import { useAuth } from '../context/AuthContext';

const Dashboard = () => {
  const { logout } = useAuth();
  const [clients, setClients] = useState([]);
  const [tags, setTags] = useState([]);
  const [rate, setRate] = useState({ value: 0 });
  const [payments, setPayments] = useState([]);
  const [newClient, setNewClient] = useState({
    name: '',
    email: '',
    balance: 0,
  });
  const [loading, setLoading] = useState(true);
  const toast = useToast();

  useEffect(() => {
    const fetchData = async () => {
      try {
        const [clientsData, rateData, paymentsData, tagsData] = await Promise.all([
          getClients(),
          getRate(),
          getPayments(),
          getTags(),
        ]);
        setClients(clientsData);
        setRate(rateData);
        setPayments(paymentsData);
        setTags(tagsData);
      } catch (error) {
        toast({
          title: error.message || 'Error',
          description: error.details || 'Failed to fetch data',
          status: 'error',
          duration: 5000,
          isClosable: true,
        });
      } finally {
        setLoading(false);
      }
    };
  
    fetchData();
  }, [toast]);

  const handleClientCreated = (client) => {
    setClients([...clients, client]);
    setNewClient({ name: '', email: '', balance: 0 });
  };

  const handleClientUpdated = (updatedClient) => {
    setClients(clients.map((client) => 
      client.id === updatedClient.id ? updatedClient : client,
    ));
  };

  const handleClientDeleted = (id) => {
    setClients(clients.filter((client) => client.id !== id));
  };

  const handleTagDeleted = (tagId) => {
    setClients(clients.map((client) => ({
      ...client,
      tags: client.tags?.filter((tag) => tag.id !== tagId) || [],
    })));
  };

  const handleTagUpdate = (updatedTags) => {
    setTags(updatedTags);
  };

  const handleAddClient = async (e) => {
    e.preventDefault();
    try {
      const createdClient = await createClient(newClient);
      handleClientCreated(createdClient);
      toast({
        title: 'Success',
        description: 'Client created successfully',
        status: 'success',
        duration: 3000,
        isClosable: true,
      });
    } catch (error) {
      toast({
        title: 'Error creating client',
        description: error.message,
        status: 'error',
        duration: 5000,
        isClosable: true,
      });
    }
  };

  if (loading) {
    return (
      <Flex justify="center" align="center" h="100vh">
        <Spinner size="xl" />
      </Flex>
    );
  }

  return (
    <Box minH="100vh" bg="gray.50">
      <Box bg="white" boxShadow="sm" py={4} px={{ base: 4, md: 8 }}>
        <Flex 
          maxW="7xl" 
          mx="auto" 
          justify="space-between" 
          align="center"
        >
          <Heading size="lg">Admin Dashboard</Heading>
          <Button 
            colorScheme="red" 
            onClick={logout}
          >
            Logout
          </Button>
        </Flex>
      </Box>

      <Box as="main" maxW="7xl" mx="auto" py={6} px={{ base: 4, md: 8 }}>
        <Grid templateColumns={{ md: '2fr 1fr' }} gap={6}>
          <GridItem>
            <Box bg="white" p={6} borderRadius="md" boxShadow="sm" mb={6}>
              <Heading size="md" mb={4}>Add New Client</Heading>
              <Stack as="form" onSubmit={handleAddClient} direction={{ base: 'column', md: 'row' }} spacing={4}>
                <FormControl>
                  <FormLabel>Name</FormLabel>
                  <Input
                    value={newClient.name}
                    onChange={(e) => setNewClient({ ...newClient, name: e.target.value })}
                    required
                  />
                </FormControl>
                <FormControl>
                  <FormLabel>Email</FormLabel>
                  <Input
                    type="email"
                    value={newClient.email}
                    onChange={(e) => setNewClient({ ...newClient, email: e.target.value })}
                    required
                  />
                </FormControl>
                <FormControl>
                  <FormLabel>Balance</FormLabel>
                  <Input
                    type="number"
                    value={newClient.balance}
                    onChange={(e) => setNewClient({ ...newClient, balance: parseFloat(e.target.value) || 0 })}
                    required
                  />
                </FormControl>
                <Button 
                  type="submit" 
                  colorScheme="green" 
                  alignSelf="flex-end"
                >
                  Add
                </Button>
              </Stack>
            </Box>

            <Box bg="white" p={6} borderRadius="md" boxShadow="sm" mb={6}>
              <Heading size="md" mb={4}>Clients</Heading>
              <ClientTable 
                clients={clients} 
                tags={tags}
                onClientUpdated={handleClientUpdated}
                onClientDeleted={handleClientDeleted}
              />
            </Box>

            <Box bg="white" p={6} borderRadius="md" boxShadow="sm">
              <TagManager tags={tags} onTagsUpdated={handleTagUpdate} onTagDeleted={handleTagDeleted} />
            </Box>
          </GridItem>

          <GridItem>
            <RateCard 
              rate={rate} 
              onRateUpdated={(newRate) => {
                setRate(newRate);
                toast({
                  title: 'Rate updated',
                  status: 'success',
                  duration: 3000,
                  isClosable: true,
                });
              }} 
            />

            <Box bg="white" p={6} borderRadius="md" boxShadow="sm" mt={6}>
              <Heading size="md" mb={4}>Recent Payments</Heading>
              <Stack spacing={4}>
                {payments.map((payment) => (
                  <Box key={payment.id} borderBottomWidth="1px" pb={2}>
                    <Text fontWeight="medium">{payment.client.name}</Text>
                    <Text color="gray.600">${payment.amount.toFixed(2)}</Text>
                    <Text fontSize="sm" color="gray.500">
                      {new Date(payment.createdAt).toLocaleString()}
                    </Text>
                  </Box>
                ))}
              </Stack>
            </Box>
          </GridItem>
        </Grid>
      </Box>
    </Box>
  );
};

export default Dashboard;