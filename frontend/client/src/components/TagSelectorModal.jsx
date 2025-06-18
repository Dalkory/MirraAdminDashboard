import {
  Modal,
  ModalOverlay,
  ModalContent,
  ModalHeader,
  ModalCloseButton,
  ModalBody,
  ModalFooter,
  Button,
  SimpleGrid,
  useToast,
  Box,
  Badge,
  Input,
  Heading,
  Text,
} from '@chakra-ui/react';
import { useState, useEffect } from 'react';

import { updateClientTags } from '../api/tags';

const TagSelectorModal = ({ 
  isOpen, 
  onClose, 
  tags, 
  clientTags = [], 
  clientId,
  onTagsUpdated,
}) => {
  const toast = useToast();
  const [animatingTag, setAnimatingTag] = useState(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedTags, setSelectedTags] = useState([]);
  const [isSaving, setIsSaving] = useState(false);

  useEffect(() => {
    if (isOpen) {
      setSelectedTags([...clientTags]);
    }
  }, [isOpen, clientTags]);

  const filteredTags = tags.filter((tag) => 
    tag.name.toLowerCase().includes(searchTerm.toLowerCase()),
  );

  const handleTagToggle = (tag) => {
    setAnimatingTag(tag.id);
    
    const isSelected = selectedTags.some((t) => t.id === tag.id);
    if (isSelected) {
      setSelectedTags(selectedTags.filter((t) => t.id !== tag.id));
    } else {
      setSelectedTags([...selectedTags, tag]);
    }
    
    setTimeout(() => setAnimatingTag(null), 200);
  };

  const handleSave = async () => {
    setIsSaving(true);
    try {
      const tagIds = selectedTags.map((t) => t.id);
      await updateClientTags(clientId, tagIds);
      onTagsUpdated(clientId, selectedTags);
      onClose();
      toast({
        title: 'Tags updated successfully',
        status: 'success',
        duration: 3000,
        isClosable: true,
      });
    } catch (error) {
      toast({
        title: 'Error updating tags',
        description: error.message,
        status: 'error',
        duration: 5000,
        isClosable: true,
      });
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} size="xl">
      <ModalOverlay />
      <ModalContent>
        <ModalHeader>
          <Box>
            Manage Client Tags
            <Box fontSize="sm" color="gray.500" fontWeight="normal">
              Changes will be saved when you click Done
            </Box>
          </Box>
        </ModalHeader>
        <ModalCloseButton />
        <ModalBody>
          <Input
            placeholder="Search tags..."
            mb={4}
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />

          <Box mb={6}>
            <Heading size="sm" mb={2}>Selected Tags ({selectedTags.length})</Heading>
            {selectedTags.length > 0 ? (
              <SimpleGrid columns={3} spacing={3} minH="50px">
                {selectedTags.map((tag) => (
                  <Box key={tag.id}>
                    <Badge
                      px={3}
                      py={1}
                      borderRadius="full"
                      backgroundColor={tag.color}
                      color="white"
                      cursor="pointer"
                      onClick={() => handleTagToggle(tag)}
                      _hover={{
                        backgroundColor: `${tag.color}90`,
                        transform: 'scale(1.05)',
                      }}
                      _active={{
                        transform: 'scale(0.95)',
                      }}
                      textAlign="center"
                      fontWeight="medium"
                      transition="all 0.2s"
                      boxShadow="md"
                      transform={animatingTag === tag.id ? 'scale(0.95)' : 'scale(1)'}
                    >
                      {tag.name} ✓
                    </Badge>
                  </Box>
                ))}
              </SimpleGrid>
            ) : (
              <Text color="gray.500">No tags selected yet</Text>
            )}
          </Box>

          <Box>
            <Heading size="sm" mb={2}>Available Tags ({filteredTags.length})</Heading>
            {filteredTags.length > 0 ? (
              <SimpleGrid columns={3} spacing={3}>
                {filteredTags
                  .filter((tag) => !selectedTags.some((t) => t.id === tag.id))
                  .map((tag) => (
                    <Box key={tag.id}>
                      <Badge
                        px={3}
                        py={1}
                        borderRadius="full"
                        backgroundColor="transparent"
                        color={tag.color}
                        border={`2px solid ${tag.color}`}
                        cursor="pointer"
                        onClick={() => handleTagToggle(tag)}
                        _hover={{
                          backgroundColor: `${tag.color}10`,
                          transform: 'scale(1.05)',
                        }}
                        _active={{
                          transform: 'scale(0.95)',
                        }}
                        textAlign="center"
                        fontWeight="medium"
                        transition="all 0.2s"
                        transform={animatingTag === tag.id ? 'scale(0.95)' : 'scale(1)'}
                      >
                        {tag.name}
                      </Badge>
                    </Box>
                  ))}
              </SimpleGrid>
            ) : (
              <Text color="gray.500">No tags available</Text>
            )}
          </Box>
        </ModalBody>
        <ModalFooter>
          <Button variant="outline" mr={3} onClick={onClose}>
            Cancel
          </Button>
          <Button 
            colorScheme="blue" 
            onClick={handleSave}
            isLoading={isSaving}
            loadingText="Saving..."
          >
            Done
          </Button>
        </ModalFooter>
      </ModalContent>
    </Modal>
  );
};

export default TagSelectorModal;