import { CloseIcon } from '@chakra-ui/icons';
import { Badge, Box, IconButton } from '@chakra-ui/react';

const TagBadge = ({ tag, onRemove, isRemovable = false }) => {
  return (
    <Badge 
      key={tag.id} 
      px={3} 
      py={1} 
      m={1}
      borderRadius="full"
      backgroundColor={tag.color}
      color="white"
      transition="all 0.2s"
      boxShadow="md"
      _hover={{
        transform: 'translateY(-2px)',
        boxShadow: 'lg',
      }}
    >
      <Box display="flex" alignItems="center">
        {tag.name}
        {isRemovable && (
          <IconButton
            icon={<CloseIcon boxSize={2} />}
            size="xs"
            ml={1}
            variant="ghost"
            color="white"
            aria-label="Remove tag"
            onClick={(e) => {
              e.stopPropagation();
              onRemove(tag.id);
            }}
          />
        )}
      </Box>
    </Badge>
  );
};

export default TagBadge;