import React, { useState } from 'react';
import {
  Container,
  Box,
  TextField,
  Button,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Typography,
  Paper,
  CircularProgress,
  Snackbar,
  Alert,
  ThemeProvider,
  createTheme,
} from '@mui/material';
import { Prism as SyntaxHighlighter } from 'react-syntax-highlighter';
import { vscDarkPlus } from 'react-syntax-highlighter/dist/esm/styles/prism';
import axios from 'axios';

const theme = createTheme({
  palette: {
    mode: 'dark',
    primary: {
      main: '#90caf9',
    },
    secondary: {
      main: '#f48fb1',
    },
    background: {
      default: '#121212',
      paper: '#1e1e1e',
    },
  },
});

function App() {
  const [code, setCode] = useState('');
  const [roastLevel, setRoastLevel] = useState('light');
  const [response, setResponse] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [conversationHistory, setConversationHistory] = useState([]);

  const handleRoast = async () => {
    if (!code.trim()) {
      setError('Please enter some code first!');
      return;
    }

    setLoading(true);
    setError('');

    try {
      const response = await axios.post('http://localhost:5000/api/roast', {
        code,
        roastLevel,
        conversationHistory,
      });

      const newMessage = {
        role: 'assistant',
        content: response.data,
      };

      setConversationHistory([...conversationHistory, newMessage]);
      setResponse(response.data);
    } catch (err) {
      setError(err.response?.data?.message || 'An error occurred while roasting your code');
    } finally {
      setLoading(false);
    }
  };

  return (
    <ThemeProvider theme={theme}>
      <Container maxWidth="lg">
        <Box sx={{ my: 4 }}>
          <Typography variant="h3" component="h1" gutterBottom align="center">
            Roast My Code
          </Typography>
          
          <Paper elevation={3} sx={{ p: 3, mb: 3 }}>
            <FormControl fullWidth sx={{ mb: 2 }}>
              <InputLabel>Roast Level</InputLabel>
              <Select
                value={roastLevel}
                label="Roast Level"
                onChange={(e) => setRoastLevel(e.target.value)}
              >
                <MenuItem value="light">Light</MenuItem>
                <MenuItem value="savage">Savage</MenuItem>
                <MenuItem value="brutal">Brutal</MenuItem>
              </Select>
            </FormControl>

            <TextField
              fullWidth
              multiline
              rows={8}
              variant="outlined"
              label="Paste your code here"
              value={code}
              onChange={(e) => setCode(e.target.value)}
              sx={{ mb: 2 }}
            />

            <Button
              variant="contained"
              color="primary"
              onClick={handleRoast}
              disabled={loading}
              fullWidth
              size="large"
            >
              {loading ? <CircularProgress size={24} /> : 'Roast My Code!'}
            </Button>
          </Paper>

          {response && (
            <Paper elevation={3} sx={{ p: 3 }}>
              <Typography variant="h6" gutterBottom>
                Roast Result:
              </Typography>
              <Box sx={{ 
                backgroundColor: '#1e1e1e',
                borderRadius: 1,
                p: 2,
                '& pre': { margin: 0 }
              }}>
                <SyntaxHighlighter
                  language="markdown"
                  style={vscDarkPlus}
                >
                  {response}
                </SyntaxHighlighter>
              </Box>
            </Paper>
          )}

          <Snackbar
            open={!!error}
            autoHideDuration={6000}
            onClose={() => setError('')}
          >
            <Alert severity="error" onClose={() => setError('')}>
              {error}
            </Alert>
          </Snackbar>
        </Box>
      </Container>
    </ThemeProvider>
  );
}

export default App; 