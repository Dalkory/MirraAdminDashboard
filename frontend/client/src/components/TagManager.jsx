import { Box, Button, FormControl, FormLabel, Input, Stack, useDisclosure, 
  Heading, 
  ButtonGroup,
} from '@chakra-ui/react';
import { useToast } from '@chakra-ui/toast';
import { useState } from 'react';

import { createTag, deleteTag, updateTag } from '../api/tags';

import ConfirmationModal from './ConfirmationModal';
import TagBadge from './TagBadge';

const TagManager = ({ tags, onTagsUpdated, onTagDeleted  }) => {
  const [newTag, setNewTag] = useState({ name: '', color: '#3182CE' });
  const [editingTag, setEditingTag] = useState(null);
  const { isOpen, onOpen, onClose } = useDisclosure();
  const [tagToDelete, setTagToDelete] = useState(null);
  const toast = useToast();

  const handleCreateTag = async () => {
    try {
      const createdTag = await createTag(newTag);
      onTagsUpdated([...tags, createdTag]);
      setNewTag({ name: '', color: '#3182CE' });
      toast({
        title: 'Tag created successfully',
        status: 'success',
        duration: 3000,
        isClosable: true,
      });
    } catch (error) {
      const errorMessage = error.details || 'Failed to create tag';
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
  
  const handleUpdateTag = async () => {
    try {
      const updatedTag = await updateTag(editingTag.id, editingTag);
      onTagsUpdated(tags.map((tag) => tag.id === updatedTag.id ? updatedTag : tag));
      setEditingTag(null);
      toast({
        title: 'Tag updated successfully',
        status: 'success',
        duration: 3000,
        isClosable: true,
      });
    } catch (error) {
      const errorMessage = error.details || 'Failed to update tag';
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

  const handleDeleteTag = async () => {
    try {
      await deleteTag(tagToDelete);
      onTagsUpdated(tags.filter((tag) => tag.id !== tagToDelete));
      onTagDeleted(tagToDelete);
      onClose();
      toast({
        title: 'Tag deleted',
        description: 'The tag has been removed from all clients',
        status: 'success',
        duration: 3000,
        isClosable: true,
      });
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to delete tag',
        status: 'error',
        duration: 5000,
        isClosable: true,
      });
    }
  };

  return (
    <Box bg="white" p={6} borderRadius="md" boxShadow="sm" mt={6}>
      <Heading size="md" mb={4}>Tag Manager</Heading>
      
      <Stack spacing={4}>
        <FormControl>
          <FormLabel>Tag Name</FormLabel>
          <Input
            value={editingTag?.name || newTag.name}
            onChange={(e) => 
              editingTag 
                ? setEditingTag({ ...editingTag, name: e.target.value })
                : setNewTag({ ...newTag, name: e.target.value })
            }
          />
        </FormControl>
        
        <FormControl>
          <FormLabel>Tag Color</FormLabel>
          <Box
            width="100%"
            height="40px"
            borderRadius="md"
            border="1px solid"
            borderColor="gray.200"
            backgroundColor={editingTag?.color || newTag.color}
            cursor="pointer"
            position="relative"
            onClick={() => document.getElementById('colorPickerHidden').click()}
          />
          <Input
            id="colorPickerHidden"
            type="color"
            value={editingTag?.color || newTag.color}
            onChange={(e) =>
              editingTag
                ? setEditingTag({ ...editingTag, color: e.target.value })
                : setNewTag({ ...newTag, color: e.target.value })
            }
            position="absolute"
            opacity={0}
            pointerEvents="none"
            width="0"
            height="0"
          />
        </FormControl>
        
        {editingTag ? (
          <ButtonGroup>
            <Button colorScheme="blue" onClick={handleUpdateTag}>
              Update Tag
            </Button>
            <Button onClick={() => setEditingTag(null)}>
              Cancel
            </Button>
          </ButtonGroup>
        ) : (
          <Button colorScheme="green" onClick={handleCreateTag}>
            Create Tag
          </Button>
        )}
      </Stack>
      
      <Box mt={6}>
        <Heading size="sm" mb={2}>Available Tags</Heading>
        <Stack direction="row" wrap="wrap">
          {tags.map((tag) => (
            <Box key={tag.id} position="relative">
              <TagBadge
                tag={tag}
                onClick={() => setEditingTag(tag)}
                cursor="pointer"
                _hover={{ opacity: 0.8 }}
                isRemovable={true}
                onRemove={() => {
                  setTagToDelete(tag.id);
                  onOpen();
                }}
              />
            </Box>
          ))}
        </Stack>
      </Box>
      
      <ConfirmationModal
        isOpen={isOpen}
        onClose={onClose}
        onConfirm={handleDeleteTag}
        title="Delete Tag"
        message="Are you sure you want to delete this tag?"
      />
    </Box>
  );
};

export default TagManager;