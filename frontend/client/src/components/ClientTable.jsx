import { Button, ButtonGroup, Input, Stack, useDisclosure, Badge } from '@chakra-ui/react';
import { Tbody, Table, Td, Th, Thead, Tr } from '@chakra-ui/table';
import { useToast } from '@chakra-ui/toast';
import { useState } from 'react';

import { deleteClient, updateClient } from '../api/clients';
import { updateClientTags } from '../api/tags';

import TagBadge from './TagBadge';
import TagSelectorModal from './TagSelectorModal';

const ClientTable = ({ clients, onClientUpdated, onClientDeleted, tags }) => {
  const [editingId, setEditingId] = useState(null);
  const [editForm, setEditForm] = useState({ name: '', email: '', balance: 0 });
  const { isOpen, onOpen, onClose } = useDisclosure();
  const [selectedClientId, setSelectedClientId] = useState(null);
  const toast = useToast();

  const handleEditClick = (client) => {
    setEditingId(client.id);
    setEditForm({
      name: client.name,
      email: client.email,
      balance: client.balance,
    });
  };

  const handleEditChange = (e) => {
    const { name, value } = e.target;
    setEditForm((prev) => ({
      ...prev,
      [name]: name === 'balance' ? parseFloat(value) || 0 : value,
    }));
  };

  const handleSave = async (id) => {
    try {
      const currentClient = clients.find((client) => client.id === id);
      const updatedClient = await updateClient(id, {
        ...editForm,
        tags: currentClient.tags,
      });
      
      onClientUpdated(updatedClient);
      setEditingId(null);
      toast({
        title: 'Client updated successfully',
        status: 'success',
        duration: 3000,
        isClosable: true,
      });
    } catch (error) {
      const errorMessage = error.details || 'Failed to update client';
      const validationErrors = error.validationErrors || {};
      
      toast({
        title: error.message || 'Validation Error',
        description: Object.values(validationErrors).join(' ') || errorMessage,
        status: 'error',
        duration: 5000,
        isClosable: true,
      });
    }
  };
  
  const handleDelete = async (id) => {
    try {
      await deleteClient(id);
      onClientDeleted(id);
      toast({
        title: 'Client deleted successfully',
        status: 'success',
        duration: 3000,
        isClosable: true,
      });
    } catch (error) {
      toast({
        title: error.message || 'Error',
        description: error.details || 'Failed to delete client',
        status: 'error',
        duration: 5000,
        isClosable: true,
      });
    }
  };

  const handleManageTags = (clientId) => {
    setSelectedClientId(clientId);
    onOpen();
  };

  const handleSaveTags = async (clientId, selectedTags) => {
    try {
      await updateClientTags(clientId, selectedTags.map((t) => t.id));
      onClientUpdated({
        ...clients.find((c) => c.id === clientId),
        tags: selectedTags,
      });
      onClose();
      toast({
        title: 'Tags updated successfully',
        status: 'success',
        duration: 3000,
        isClosable: true,
      });
    } catch (error) {
      toast({
        title: error.message || 'Error',
        description: error.details || 'Failed to update tags',
        status: 'error',
        duration: 5000,
        isClosable: true,
      });
    }
  };

  const handleRemoveTag = async (clientId, tagId) => {
    try {
      const client = clients.find((c) => c.id === clientId);
      if (!client) return;

      const updatedTags = client.tags?.filter((tag) => tag.id !== tagId) || [];
      await updateClientTags(clientId, updatedTags.map((t) => t.id));
      
      onClientUpdated({
        ...client,
        tags: updatedTags,
      });
      
      toast({
        title: 'Tag removed',
        status: 'success',
        duration: 3000,
        isClosable: true,
      });
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to remove tag',
        status: 'error',
        duration: 5000,
        isClosable: true,
      });
    }
  };

  return (
    <>
      <Table variant="simple">
        <Thead>
          <Tr>
            <Th>Name</Th>
            <Th>Email</Th>
            <Th>Balance</Th>
            <Th>Tags</Th>
            <Th>Actions</Th>
          </Tr>
        </Thead>
        <Tbody>
          {clients.map((client) => (
            <Tr key={client.id}>
              <Td>
                {editingId === client.id ? (
                  <Input
                    name="name"
                    value={editForm.name}
                    onChange={handleEditChange}
                  />
                ) : (
                  client.name
                )}
              </Td>
              <Td>
                {editingId === client.id ? (
                  <Input
                    name="email"
                    value={editForm.email}
                    onChange={handleEditChange}
                  />
                ) : (
                  client.email
                )}
              </Td>
              <Td>
                {editingId === client.id ? (
                  <Input
                    name="balance"
                    type="number"
                    value={editForm.balance}
                    onChange={handleEditChange}
                  />
                ) : (
                  client.balance
                )}
              </Td>
              <Td>
                <Stack direction="row" wrap="wrap">
                  {client.tags?.map((tag) => (
                    <TagBadge
                      key={tag.id}
                      tag={tag}
                      isSelected={true}
                      onRemove={(tagId) => handleRemoveTag(client.id, tagId)}
                      isRemovable
                    />
                  ))}
                  {(!client.tags || client.tags.length === 0) && (
                    <Badge 
                      colorScheme="gray" 
                      variant="subtle" 
                      px={2} 
                      py={1} 
                      m={1}
                      borderRadius="full"
                    >
                      No tags
                    </Badge>
                  )}
                </Stack>
              </Td>
              <Td>
                {editingId === client.id ? (
                  <ButtonGroup size="sm">
                    <Button 
                      colorScheme="green"
                      onClick={() => handleSave(client.id)}
                    >
                      Save
                    </Button>
                    <Button 
                      onClick={() => setEditingId(null)}
                    >
                      Cancel
                    </Button>
                  </ButtonGroup>
                ) : (
                  <ButtonGroup size="sm">
                    <Button 
                      colorScheme="blue"
                      onClick={() => handleEditClick(client)}
                    >
                      Edit
                    </Button>
                    <Button 
                      colorScheme="teal"
                      onClick={() => handleManageTags(client.id)}
                    >
                      Tags
                    </Button>
                    <Button 
                      colorScheme="red"
                      onClick={() => handleDelete(client.id)}
                    >
                      Delete
                    </Button>
                  </ButtonGroup>
                )}
              </Td>
            </Tr>
          ))}
        </Tbody>
      </Table>

      <TagSelectorModal
        isOpen={isOpen}
        onClose={onClose}
        tags={tags}
        clientTags={clients.find((c) => c.id === selectedClientId)?.tags || []}
        clientId={selectedClientId}
        onTagsUpdated={handleSaveTags}
      />
    </>
  );
};

export default ClientTable;