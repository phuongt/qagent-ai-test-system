# QAgent Frontend API Integration Guide

Hướng dẫn chi tiết để tích hợp QAgent API với các ứng dụng frontend (React, Vue, Angular, etc.)

## Quick Start

### 1. Setup Base Configuration

```javascript
// api-config.js
const API_BASE_URL = 'https://your-qagent-domain.com/api';

const apiClient = {
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json'
  },
  credentials: 'include' // Important for session cookies
};

export default apiClient;
```

### 2. Authentication Service

```javascript
// auth-service.js
import apiClient from './api-config';

class AuthService {
  async login(email, password, rememberMe = false) {
    const response = await fetch(`${apiClient.baseURL}/Auth/login`, {
      method: 'POST',
      headers: apiClient.headers,
      credentials: apiClient.credentials,
      body: JSON.stringify({ email, password, rememberMe })
    });
    
    if (!response.ok) {
      throw new Error('Login failed');
    }
    
    return await response.json();
  }

  async register(userData) {
    const response = await fetch(`${apiClient.baseURL}/Auth/register`, {
      method: 'POST',
      headers: apiClient.headers,
      credentials: apiClient.credentials,
      body: JSON.stringify(userData)
    });
    
    return await response.json();
  }

  async getCurrentUser() {
    const response = await fetch(`${apiClient.baseURL}/Auth/me`, {
      headers: apiClient.headers,
      credentials: apiClient.credentials
    });
    
    if (response.ok) {
      return await response.json();
    }
    return null;
  }

  async logout() {
    await fetch(`${apiClient.baseURL}/Auth/logout`, {
      method: 'POST',
      headers: apiClient.headers,
      credentials: apiClient.credentials
    });
  }
}

export default new AuthService();
```

### 3. Project Service

```javascript
// project-service.js
import apiClient from './api-config';

class ProjectService {
  async getProjects() {
    const response = await fetch(`${apiClient.baseURL}/Projects`, {
      headers: apiClient.headers,
      credentials: apiClient.credentials
    });
    
    if (!response.ok) {
      throw new Error('Failed to fetch projects');
    }
    
    return await response.json();
  }

  async getProject(id) {
    const response = await fetch(`${apiClient.baseURL}/Projects/${id}`, {
      headers: apiClient.headers,
      credentials: apiClient.credentials
    });
    
    return await response.json();
  }

  async createProject(projectData) {
    const response = await fetch(`${apiClient.baseURL}/Projects`, {
      method: 'POST',
      headers: apiClient.headers,
      credentials: apiClient.credentials,
      body: JSON.stringify(projectData)
    });
    
    return await response.json();
  }

  async updateProject(id, projectData) {
    const response = await fetch(`${apiClient.baseURL}/Projects/${id}`, {
      method: 'PUT',
      headers: apiClient.headers,
      credentials: apiClient.credentials,
      body: JSON.stringify(projectData)
    });
    
    if (!response.ok) {
      throw new Error('Failed to update project');
    }
  }

  async deleteProject(id) {
    const response = await fetch(`${apiClient.baseURL}/Projects/${id}`, {
      method: 'DELETE',
      headers: apiClient.headers,
      credentials: apiClient.credentials
    });
    
    if (!response.ok) {
      throw new Error('Failed to delete project');
    }
  }
}

export default new ProjectService();
```

### 4. Screen Service

```javascript
// screen-service.js
import apiClient from './api-config';

class ScreenService {
  async getScreens(projectId = null) {
    const url = projectId 
      ? `${apiClient.baseURL}/Screens?projectId=${projectId}`
      : `${apiClient.baseURL}/Screens`;
      
    const response = await fetch(url, {
      headers: apiClient.headers,
      credentials: apiClient.credentials
    });
    
    return await response.json();
  }

  async uploadScreen(screenData) {
    const formData = new FormData();
    formData.append('name', screenData.name);
    formData.append('description', screenData.description || '');
    formData.append('projectId', screenData.projectId);
    formData.append('screenType', screenData.screenType || 'UI');
    formData.append('priority', screenData.priority || 'Medium');
    formData.append('file', screenData.file);

    const response = await fetch(`${apiClient.baseURL}/Screens`, {
      method: 'POST',
      credentials: apiClient.credentials,
      body: formData
    });
    
    return await response.json();
  }

  async uploadMultipleScreens(projectId, files, description = '') {
    const formData = new FormData();
    formData.append('projectId', projectId);
    formData.append('description', description);
    
    files.forEach(file => {
      formData.append('files', file);
    });

    const response = await fetch(`${apiClient.baseURL}/Screens/upload`, {
      method: 'POST',
      credentials: apiClient.credentials,
      body: formData
    });
    
    return await response.json();
  }
}

export default new ScreenService();
```

### 5. Analysis Service

```javascript
// analysis-service.js
import apiClient from './api-config';

class AnalysisService {
  async analyzeScreen(analysisRequest) {
    const response = await fetch(`${apiClient.baseURL}/Analysis/analyze`, {
      method: 'POST',
      headers: apiClient.headers,
      credentials: apiClient.credentials,
      body: JSON.stringify(analysisRequest)
    });
    
    return await response.json();
  }

  async getAnalysisResult(analysisId) {
    const response = await fetch(`${apiClient.baseURL}/Analysis/result/${analysisId}`, {
      headers: apiClient.headers,
      credentials: apiClient.credentials
    });
    
    return await response.json();
  }

  async batchAnalyze(batchRequest) {
    const response = await fetch(`${apiClient.baseURL}/Analysis/batch`, {
      method: 'POST',
      headers: apiClient.headers,
      credentials: apiClient.credentials,
      body: JSON.stringify(batchRequest)
    });
    
    return await response.json();
  }

  async pollAnalysisProgress(analysisId, onProgress) {
    const poll = async () => {
      try {
        const result = await this.getAnalysisResult(analysisId);
        
        if (result.analysisStatus === 'Completed' || result.analysisStatus === 'Failed') {
          onProgress(result);
          return result;
        }
        
        onProgress(result);
        setTimeout(poll, 2000); // Poll every 2 seconds
      } catch (error) {
        console.error('Error polling analysis progress:', error);
      }
    };
    
    poll();
  }
}

export default new AnalysisService();
```

## React Examples

### 1. Login Component

```jsx
// components/Login.jsx
import React, { useState } from 'react';
import AuthService from '../services/auth-service';

function Login() {
  const [credentials, setCredentials] = useState({ email: '', password: '' });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError('');

    try {
      const result = await AuthService.login(credentials.email, credentials.password);
      
      if (result.success) {
        // Redirect to dashboard
        window.location.href = '/dashboard';
      } else {
        setError(result.message || 'Login failed');
      }
    } catch (err) {
      setError('Login failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <div>
        <input
          type="email"
          placeholder="Email"
          value={credentials.email}
          onChange={(e) => setCredentials({ ...credentials, email: e.target.value })}
          required
        />
      </div>
      <div>
        <input
          type="password"
          placeholder="Password"
          value={credentials.password}
          onChange={(e) => setCredentials({ ...credentials, password: e.target.value })}
          required
        />
      </div>
      {error && <div className="error">{error}</div>}
      <button type="submit" disabled={loading}>
        {loading ? 'Logging in...' : 'Login'}
      </button>
    </form>
  );
}

export default Login;
```

### 2. Project List Component

```jsx
// components/ProjectList.jsx
import React, { useState, useEffect } from 'react';
import ProjectService from '../services/project-service';

function ProjectList() {
  const [projects, setProjects] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadProjects();
  }, []);

  const loadProjects = async () => {
    try {
      const data = await ProjectService.getProjects();
      setProjects(data);
    } catch (error) {
      console.error('Failed to load projects:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (projectId) => {
    if (window.confirm('Are you sure you want to delete this project?')) {
      try {
        await ProjectService.deleteProject(projectId);
        loadProjects(); // Reload list
      } catch (error) {
        alert('Failed to delete project');
      }
    }
  };

  if (loading) return <div>Loading projects...</div>;

  return (
    <div>
      <h2>Projects</h2>
      {projects.length === 0 ? (
        <p>No projects found.</p>
      ) : (
        <div className="project-grid">
          {projects.map(project => (
            <div key={project.id} className="project-card">
              <h3>{project.name}</h3>
              <p>{project.description}</p>
              <div className="project-meta">
                <span>Domain: {project.domain}</span>
                <span>Status: {project.status}</span>
                <span>Screens: {project.screenCount}</span>
              </div>
              <div className="project-actions">
                <button onClick={() => window.location.href = `/projects/${project.id}`}>
                  View
                </button>
                <button onClick={() => handleDelete(project.id)}>Delete</button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

export default ProjectList;
```

### 3. Screen Upload Component

```jsx
// components/ScreenUpload.jsx
import React, { useState } from 'react';
import ScreenService from '../services/screen-service';

function ScreenUpload({ projectId, onUploadComplete }) {
  const [selectedFiles, setSelectedFiles] = useState([]);
  const [uploading, setUploading] = useState(false);
  const [uploadProgress, setUploadProgress] = useState(0);

  const handleFileSelect = (e) => {
    const files = Array.from(e.target.files);
    setSelectedFiles(files);
  };

  const handleUpload = async () => {
    if (selectedFiles.length === 0) return;

    setUploading(true);
    setUploadProgress(0);

    try {
      const result = await ScreenService.uploadMultipleScreens(
        projectId, 
        selectedFiles,
        'Batch upload'
      );

      if (result.screens) {
        setUploadProgress(100);
        onUploadComplete?.(result.screens);
        setSelectedFiles([]);
        
        // Show success message
        alert(`Successfully uploaded ${result.screens.length} screens!`);
      }
    } catch (error) {
      console.error('Upload failed:', error);
      alert('Upload failed. Please try again.');
    } finally {
      setUploading(false);
    }
  };

  return (
    <div className="screen-upload">
      <h3>Upload Screens</h3>
      
      <div className="file-input">
        <input
          type="file"
          multiple
          accept=".jpg,.jpeg,.png,.pdf,.bmp,.gif,.tiff"
          onChange={handleFileSelect}
          disabled={uploading}
        />
      </div>

      {selectedFiles.length > 0 && (
        <div className="selected-files">
          <h4>Selected Files ({selectedFiles.length}):</h4>
          <ul>
            {selectedFiles.map((file, index) => (
              <li key={index}>
                {file.name} ({(file.size / 1024 / 1024).toFixed(2)} MB)
              </li>
            ))}
          </ul>
        </div>
      )}

      <button 
        onClick={handleUpload} 
        disabled={selectedFiles.length === 0 || uploading}
        className="upload-btn"
      >
        {uploading ? 'Uploading...' : 'Upload Screens'}
      </button>

      {uploading && (
        <div className="progress-bar">
          <div 
            className="progress-fill" 
            style={{ width: `${uploadProgress}%` }}
          ></div>
        </div>
      )}
    </div>
  );
}

export default ScreenUpload;
```

### 4. Analysis Component

```jsx
// components/AnalysisRunner.jsx
import React, { useState } from 'react';
import AnalysisService from '../services/analysis-service';

function AnalysisRunner({ screenId, screenName }) {
  const [analysisConfig, setAnalysisConfig] = useState({
    businessDescription: '',
    analysisType: 'Full',
    includeUIElements: true,
    includeTextExtraction: true,
    includeISTQBRules: true,
    generateChecklist: true
  });
  const [analysisResult, setAnalysisResult] = useState(null);
  const [analyzing, setAnalyzing] = useState(false);

  const startAnalysis = async () => {
    setAnalyzing(true);
    setAnalysisResult(null);

    try {
      const result = await AnalysisService.analyzeScreen({
        screenId,
        ...analysisConfig
      });

      if (result.id) {
        // Poll for progress
        AnalysisService.pollAnalysisProgress(result.id, (progress) => {
          setAnalysisResult(progress);
          
          if (progress.analysisStatus === 'Completed' || progress.analysisStatus === 'Failed') {
            setAnalyzing(false);
          }
        });
      }
    } catch (error) {
      console.error('Analysis failed:', error);
      setAnalyzing(false);
      alert('Analysis failed to start');
    }
  };

  return (
    <div className="analysis-runner">
      <h3>Analyze Screen: {screenName}</h3>

      <div className="analysis-config">
        <div className="form-group">
          <label>Business Description:</label>
          <textarea
            value={analysisConfig.businessDescription}
            onChange={(e) => setAnalysisConfig({
              ...analysisConfig,
              businessDescription: e.target.value
            })}
            placeholder="Describe the business context and purpose of this screen..."
            rows={4}
          />
        </div>

        <div className="analysis-options">
          <label>
            <input
              type="checkbox"
              checked={analysisConfig.includeUIElements}
              onChange={(e) => setAnalysisConfig({
                ...analysisConfig,
                includeUIElements: e.target.checked
              })}
            />
            Include UI Elements Detection
          </label>

          <label>
            <input
              type="checkbox"
              checked={analysisConfig.includeTextExtraction}
              onChange={(e) => setAnalysisConfig({
                ...analysisConfig,
                includeTextExtraction: e.target.checked
              })}
            />
            Include Text Extraction
          </label>

          <label>
            <input
              type="checkbox"
              checked={analysisConfig.includeISTQBRules}
              onChange={(e) => setAnalysisConfig({
                ...analysisConfig,
                includeISTQBRules: e.target.checked
              })}
            />
            Apply ISTQB Rules
          </label>

          <label>
            <input
              type="checkbox"
              checked={analysisConfig.generateChecklist}
              onChange={(e) => setAnalysisConfig({
                ...analysisConfig,
                generateChecklist: e.target.checked
              })}
            />
            Generate Test Checklist
          </label>
        </div>
      </div>

      <button 
        onClick={startAnalysis} 
        disabled={analyzing}
        className="analyze-btn"
      >
        {analyzing ? 'Analyzing...' : 'Start Analysis'}
      </button>

      {analysisResult && (
        <div className="analysis-result">
          <h4>Analysis Status: {analysisResult.analysisStatus}</h4>
          
          {analysisResult.analysisStatus === 'Processing' && (
            <div className="progress">
              <div className="spinner"></div>
              <p>Analysis in progress...</p>
            </div>
          )}

          {analysisResult.analysisStatus === 'Completed' && (
            <div className="analysis-completed">
              <p>✅ Analysis completed successfully!</p>
              
              {analysisResult.uiElements && analysisResult.uiElements.length > 0 && (
                <div className="ui-elements">
                  <h5>UI Elements Detected ({analysisResult.uiElements.length}):</h5>
                  <ul>
                    {analysisResult.uiElements.slice(0, 5).map((element, index) => (
                      <li key={index}>
                        {element.type}: {element.text}
                      </li>
                    ))}
                    {analysisResult.uiElements.length > 5 && (
                      <li>... and {analysisResult.uiElements.length - 5} more</li>
                    )}
                  </ul>
                </div>
              )}

              {analysisResult.checklistItems && analysisResult.checklistItems.length > 0 && (
                <div className="checklist-items">
                  <h5>Generated Checklist ({analysisResult.checklistItems.length} items):</h5>
                  <ul>
                    {analysisResult.checklistItems.slice(0, 3).map((item, index) => (
                      <li key={index}>
                        <strong>{item.title}</strong>
                        <p>{item.description}</p>
                      </li>
                    ))}
                    {analysisResult.checklistItems.length > 3 && (
                      <li>... and {analysisResult.checklistItems.length - 3} more items</li>
                    )}
                  </ul>
                </div>
              )}
            </div>
          )}

          {analysisResult.analysisStatus === 'Failed' && (
            <div className="analysis-failed">
              <p>❌ Analysis failed</p>
              {analysisResult.errorMessage && (
                <p className="error-message">{analysisResult.errorMessage}</p>
              )}
            </div>
          )}
        </div>
      )}
    </div>
  );
}

export default AnalysisRunner;
```

## Vue.js Examples

### 1. Composable for API calls

```javascript
// composables/useQAgentAPI.js
import { ref, reactive } from 'vue';
import apiClient from '../services/api-config';

export function useQAgentAPI() {
  const loading = ref(false);
  const error = ref(null);

  const makeRequest = async (url, options = {}) => {
    loading.value = true;
    error.value = null;

    try {
      const response = await fetch(`${apiClient.baseURL}${url}`, {
        headers: apiClient.headers,
        credentials: apiClient.credentials,
        ...options
      });

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const data = await response.json();
      return data;
    } catch (err) {
      error.value = err.message;
      throw err;
    } finally {
      loading.value = false;
    }
  };

  return {
    loading,
    error,
    makeRequest
  };
}
```

### 2. Project Management Component

```vue
<!-- components/ProjectManager.vue -->
<template>
  <div class="project-manager">
    <h2>Project Management</h2>
    
    <!-- Create Project Form -->
    <form @submit.prevent="createProject" class="create-form">
      <input 
        v-model="newProject.name" 
        placeholder="Project Name" 
        required 
      />
      <input 
        v-model="newProject.description" 
        placeholder="Description" 
      />
      <select v-model="newProject.domain" required>
        <option value="">Select Domain</option>
        <option value="Web Application">Web Application</option>
        <option value="Mobile App">Mobile App</option>
        <option value="Desktop App">Desktop App</option>
      </select>
      <button type="submit" :disabled="loading">
        {{ loading ? 'Creating...' : 'Create Project' }}
      </button>
    </form>

    <!-- Projects List -->
    <div v-if="error" class="error">{{ error }}</div>
    
    <div v-if="projects.length > 0" class="projects-grid">
      <div 
        v-for="project in projects" 
        :key="project.id" 
        class="project-card"
      >
        <h3>{{ project.name }}</h3>
        <p>{{ project.description }}</p>
        <div class="project-meta">
          <span>{{ project.domain }}</span>
          <span>{{ project.status }}</span>
          <span>{{ project.screenCount }} screens</span>
        </div>
        <div class="actions">
          <button @click="viewProject(project.id)">View</button>
          <button @click="deleteProject(project.id)">Delete</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue';
import { useQAgentAPI } from '../composables/useQAgentAPI';

const { loading, error, makeRequest } = useQAgentAPI();

const projects = ref([]);
const newProject = reactive({
  name: '',
  description: '',
  domain: ''
});

const loadProjects = async () => {
  try {
    const data = await makeRequest('/Projects');
    projects.value = data;
  } catch (err) {
    console.error('Failed to load projects:', err);
  }
};

const createProject = async () => {
  try {
    await makeRequest('/Projects', {
      method: 'POST',
      body: JSON.stringify(newProject)
    });
    
    // Reset form and reload
    Object.assign(newProject, { name: '', description: '', domain: '' });
    await loadProjects();
  } catch (err) {
    console.error('Failed to create project:', err);
  }
};

const deleteProject = async (id) => {
  if (confirm('Are you sure you want to delete this project?')) {
    try {
      await makeRequest(`/Projects/${id}`, { method: 'DELETE' });
      await loadProjects();
    } catch (err) {
      console.error('Failed to delete project:', err);
    }
  }
};

const viewProject = (id) => {
  // Navigate to project detail
  // router.push(`/projects/${id}`)
};

onMounted(loadProjects);
</script>
```

## Error Handling Best Practices

### 1. Global Error Handler

```javascript
// utils/error-handler.js
export class APIError extends Error {
  constructor(message, status, response) {
    super(message);
    this.status = status;
    this.response = response;
  }
}

export const handleAPIError = (error) => {
  if (error.status === 401) {
    // Redirect to login
    window.location.href = '/login';
    return;
  }
  
  if (error.status === 403) {
    alert('You do not have permission to perform this action.');
    return;
  }
  
  if (error.status >= 500) {
    alert('Server error. Please try again later.');
    return;
  }
  
  // Default error handling
  alert(error.message || 'An unexpected error occurred.');
};
```

### 2. Enhanced API Client

```javascript
// services/enhanced-api-client.js
import { APIError, handleAPIError } from '../utils/error-handler';

class EnhancedAPIClient {
  constructor(baseURL) {
    this.baseURL = baseURL;
    this.defaultHeaders = {
      'Content-Type': 'application/json'
    };
  }

  async request(endpoint, options = {}) {
    const url = `${this.baseURL}${endpoint}`;
    const config = {
      headers: { ...this.defaultHeaders, ...options.headers },
      credentials: 'include',
      ...options
    };

    try {
      const response = await fetch(url, config);
      
      if (!response.ok) {
        const errorData = await response.json().catch(() => null);
        throw new APIError(
          errorData?.message || 'Request failed',
          response.status,
          errorData
        );
      }

      // Handle empty responses
      if (response.status === 204) {
        return null;
      }

      return await response.json();
    } catch (error) {
      if (error instanceof APIError) {
        handleAPIError(error);
        throw error;
      }
      
      // Network or other errors
      console.error('API request failed:', error);
      throw new APIError('Network error. Please check your connection.');
    }
  }

  // Convenience methods
  get(endpoint, params = {}) {
    const queryString = new URLSearchParams(params).toString();
    const url = queryString ? `${endpoint}?${queryString}` : endpoint;
    return this.request(url);
  }

  post(endpoint, data) {
    return this.request(endpoint, {
      method: 'POST',
      body: JSON.stringify(data)
    });
  }

  put(endpoint, data) {
    return this.request(endpoint, {
      method: 'PUT',
      body: JSON.stringify(data)
    });
  }

  delete(endpoint) {
    return this.request(endpoint, { method: 'DELETE' });
  }

  // File upload method
  uploadFile(endpoint, formData) {
    return this.request(endpoint, {
      method: 'POST',
      headers: {}, // Let browser set Content-Type for multipart
      body: formData
    });
  }
}

export default new EnhancedAPIClient('https://your-qagent-domain.com/api');
```

## Performance Optimization

### 1. Request Caching

```javascript
// utils/request-cache.js
class RequestCache {
  constructor() {
    this.cache = new Map();
    this.ttl = 5 * 60 * 1000; // 5 minutes
  }

  getKey(url, options) {
    return `${url}_${JSON.stringify(options)}`;
  }

  get(url, options) {
    const key = this.getKey(url, options);
    const cached = this.cache.get(key);
    
    if (cached && Date.now() - cached.timestamp < this.ttl) {
      return cached.data;
    }
    
    return null;
  }

  set(url, options, data) {
    const key = this.getKey(url, options);
    this.cache.set(key, {
      data,
      timestamp: Date.now()
    });
  }

  clear() {
    this.cache.clear();
  }
}

export default new RequestCache();
```

### 2. Debounced Search

```javascript
// utils/debounce.js
export function debounce(func, delay) {
  let timeoutId;
  return function (...args) {
    clearTimeout(timeoutId);
    timeoutId = setTimeout(() => func.apply(this, args), delay);
  };
}

// Usage in search component
const debouncedSearch = debounce(async (query) => {
  if (query.length > 2) {
    const results = await apiClient.get('/Projects', { search: query });
    setSearchResults(results);
  }
}, 300);
```

## Testing

### 1. Mock API Service

```javascript
// __mocks__/api-service.js
export const mockProjects = [
  {
    id: 1,
    name: "Test Project 1",
    description: "Test Description",
    domain: "Web Application",
    status: "Active",
    screenCount: 5
  }
];

export const mockAPIService = {
  getProjects: jest.fn(() => Promise.resolve(mockProjects)),
  createProject: jest.fn((data) => Promise.resolve({ id: 2, ...data })),
  deleteProject: jest.fn(() => Promise.resolve())
};
```

### 2. Component Test Example

```javascript
// __tests__/ProjectList.test.js
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import ProjectList from '../components/ProjectList';
import { mockAPIService } from '../__mocks__/api-service';

jest.mock('../services/project-service', () => mockAPIService);

describe('ProjectList', () => {
  test('renders projects correctly', async () => {
    render(<ProjectList />);
    
    await waitFor(() => {
      expect(screen.getByText('Test Project 1')).toBeInTheDocument();
    });
    
    expect(mockAPIService.getProjects).toHaveBeenCalled();
  });

  test('handles project deletion', async () => {
    render(<ProjectList />);
    
    await waitFor(() => {
      expect(screen.getByText('Test Project 1')).toBeInTheDocument();
    });
    
    const deleteButton = screen.getByText('Delete');
    fireEvent.click(deleteButton);
    
    // Mock window.confirm
    window.confirm = jest.fn(() => true);
    
    await waitFor(() => {
      expect(mockAPIService.deleteProject).toHaveBeenCalledWith(1);
    });
  });
});
```

## Security Considerations

### 1. CSRF Protection

QAgent API sử dụng session-based authentication với CSRF protection. Ensure:

- Always include `credentials: 'include'` in fetch requests
- Handle 403 responses appropriately
- Implement proper logout functionality

### 2. Input Validation

```javascript
// utils/validation.js
export const validateProject = (project) => {
  const errors = {};
  
  if (!project.name || project.name.trim().length < 3) {
    errors.name = 'Project name must be at least 3 characters';
  }
  
  if (!project.domain) {
    errors.domain = 'Domain is required';
  }
  
  return {
    isValid: Object.keys(errors).length === 0,
    errors
  };
};
```

### 3. File Upload Security

```javascript
// utils/file-validation.js
export const validateScreenFile = (file) => {
  const allowedTypes = [
    'image/jpeg', 'image/jpg', 'image/png', 
    'image/bmp', 'image/gif', 'image/tiff',
    'application/pdf'
  ];
  
  const maxSize = 50 * 1024 * 1024; // 50MB
  
  if (!allowedTypes.includes(file.type)) {
    throw new Error('File type not supported');
  }
  
  if (file.size > maxSize) {
    throw new Error('File size exceeds 50MB limit');
  }
  
  return true;
};
```

This integration guide provides comprehensive examples for connecting to QAgent API from various frontend frameworks. Remember to adapt the code to your specific requirements and always handle errors gracefully.