import { Box, Heading, Button, Input, ButtonGroup } from '@chakra-ui/react';
import { useToast } from '@chakra-ui/toast';
import { useState } from 'react';

import { updateRate } from '../api/rate';

const RateCard = ({ rate, onRateUpdated }) => {
  const [isEditing, setIsEditing] = useState(false);
  const [newRate, setNewRate] = useState(rate.value);
  const toast = useToast();

  const handleUpdate = async () => {
    try {
      const updatedRate = await updateRate(newRate);
      onRateUpdated(updatedRate);
      setIsEditing(false);
      toast({ title: 'Rate updated!', status: 'success', duration: 3000, isClosable: true });
    } catch (error) {
      toast({ title: 'Error updating rate!', status: 'error', duration: 5000, isClosable: true });
    }
  };

  return (
    <Box p={6} shadow="md" rounded="lg" className="bg-gray-50">
      <Heading size="md" mb={4}>
        Token Rate
      </Heading>
      {isEditing ? (
        <div className="flex items-center">
          <Input
            type="number"
            step="0.01"
            value={newRate}
            onChange={(e) => setNewRate(parseFloat(e.target.value) || 0)}
            mr={2}
            width="100px"
          />
          <ButtonGroup size="sm">
            <Button colorScheme="green" onClick={handleUpdate}>
              Save
            </Button>
            <Button onClick={() => setIsEditing(false)}>Cancel</Button>
          </ButtonGroup>
        </div>
      ) : (
        <div className="flex items-center justify-between">
          <span className="text-2xl font-semibold">
            1 T = {rate.value} USD
          </span>
          <Button 
            colorScheme="blue" 
            onClick={() => setIsEditing(true)}
            ml="auto"
            marginLeft={'10px'}
          >
            Edit Rate
          </Button>
        </div>
      )}

    </Box>
  );
};

export default RateCard;