'use client';
import { useState } from 'react';
import styles from './page.module.css';
import { useSession, signIn, signOut } from "next-auth/react";

export default function CustomerForm() {
  const { data: session, status } = useSession();
  const [formData, setFormData] = useState({
    Gender: 'Male',
    Age: '',
    AnnualIncome: '',
    SpendingScore: '',
    Profession: '',
    WorkExperience: '',
    FamilySize: ''
  });

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const sendLog = async (level: string, message: string, meta: Record<string, unknown> = {}) => {
    try {
      await fetch('/api/logs', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ level, message, meta })
      });
    } catch (e) {
      console.error('Failed to send log', e);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const response = await fetch('http://localhost:8080/customer', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          Gender: formData.Gender,
          Age: parseInt(formData.Age),
          AnnualIncome: parseInt(formData.AnnualIncome),
          SpendingScore: parseInt(formData.SpendingScore),
          Profession: formData.Profession,
          WorkExperience: parseInt(formData.WorkExperience),
          FamilySize: parseInt(formData.FamilySize)
        }),
      });

      if (response.ok) {
        await sendLog('info', 'Customer successfully loaded and queued via API', { formData });
        alert('Customer loaded successfully! It is now queued for processing.');
        setFormData({
          Gender: 'Male',
          Age: '',
          AnnualIncome: '',
          SpendingScore: '',
          Profession: '',
          WorkExperience: '',
          FamilySize: ''
        });
      } else {
        await sendLog('error', 'Failed to load customer', { status: response.status });
        alert('Failed to load customer. Please check the console.');
        console.error(await response.text());
      }
    } catch (error) {
      await sendLog('error', 'Error submitting customer request', { error: String(error) });
      console.error('Error submitting customer:', error);
      alert('An error occurred. Make sure the API is running.');
    }
  };

  const handleFileUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    
    const uploadData = new FormData();
    uploadData.append('file', file);
    
    try {
      const response = await fetch('http://localhost:8080/upload', {
        method: 'POST',
        body: uploadData,
      });

      if (response.ok) {
        const msg = await response.text();
        await sendLog('info', 'CSV uploaded successfully', { message: msg });
        alert(`Upload successful: ${msg}`);
      } else {
        await sendLog('error', 'Failed to upload CSV', { status: response.status });
        alert('Failed to upload CSV. Please check the console.');
      }
    } catch (error) {
      await sendLog('error', 'Error uploading file', { error: String(error) });
      console.error('Error uploading file:', error);
      alert('An error occurred while uploading the file.');
    }
  };

  if (status === "loading") {
    return (
      <main className={styles.main}>
        <div className={styles.formContainer} style={{ textAlign: 'center' }}>
          <p>Loading...</p>
        </div>
      </main>
    );
  }

  if (!session) {
    return (
      <main className={styles.main}>
        <div className={styles.formContainer} style={{ textAlign: 'center' }}>
          <h1 className={styles.title}>Welcome</h1>
          <p className={styles.subtitle}>Please sign in to access the customer pipeline.</p>
          <button 
            onClick={() => signIn('google')} 
            className={styles.submitBtn}
            style={{ marginTop: '2rem' }}
          >
            Sign in with Google
          </button>
        </div>
      </main>
    );
  }

  return (
    <main className={styles.main}>
      <div className={styles.formContainer}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
          <div>
            <h1 className={styles.title}>Load Customer</h1>
            <p className={styles.subtitle}>Signed in as {session.user?.email}</p>
          </div>
          <button 
            onClick={() => signOut()} 
            style={{ background: 'transparent', border: '1px solid rgba(255,255,255,0.2)', color: 'white', padding: '0.5rem 1rem', borderRadius: '8px', cursor: 'pointer' }}
          >
            Sign Out
          </button>
        </div>
        
        <form onSubmit={handleSubmit}>
          <div className={styles.grid}>
            
            <div className={styles.inputGroup}>
              <label className={styles.label}>Gender</label>
              <select 
                name="Gender" 
                className={`${styles.input} ${styles.select}`}
                value={formData.Gender}
                onChange={handleChange}
              >
                <option value="Male">Male</option>
                <option value="Female">Female</option>
                <option value="Other">Other</option>
              </select>
            </div>

            <div className={styles.inputGroup}>
              <label className={styles.label}>Age</label>
              <input 
                type="number" 
                name="Age" 
                className={styles.input} 
                placeholder="e.g. 28" 
                required 
                value={formData.Age}
                onChange={handleChange}
              />
            </div>

            <div className={styles.inputGroup}>
              <label className={styles.label}>Annual Income ($)</label>
              <input 
                type="number" 
                name="AnnualIncome" 
                className={styles.input} 
                placeholder="e.g. 75000" 
                required 
                value={formData.AnnualIncome}
                onChange={handleChange}
              />
            </div>

            <div className={styles.inputGroup}>
              <label className={styles.label}>Spending Score (1-100)</label>
              <input 
                type="number" 
                name="SpendingScore" 
                className={styles.input} 
                placeholder="e.g. 85" 
                required 
                min="1" max="100"
                value={formData.SpendingScore}
                onChange={handleChange}
              />
            </div>

            <div className={styles.inputGroup}>
              <label className={styles.label}>Profession</label>
              <input 
                type="text" 
                name="Profession" 
                className={styles.input} 
                placeholder="e.g. Engineer" 
                required 
                value={formData.Profession}
                onChange={handleChange}
              />
            </div>

            <div className={styles.inputGroup}>
              <label className={styles.label}>Work Experience (Years)</label>
              <input 
                type="number" 
                name="WorkExperience" 
                className={styles.input} 
                placeholder="e.g. 5" 
                required 
                value={formData.WorkExperience}
                onChange={handleChange}
              />
            </div>

            <div className={`${styles.inputGroup} ${styles.fullWidth}`}>
              <label className={styles.label}>Family Size</label>
              <input 
                type="number" 
                name="FamilySize" 
                className={styles.input} 
                placeholder="e.g. 3" 
                required 
                value={formData.FamilySize}
                onChange={handleChange}
              />
            </div>
            
          </div>
          
          <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem', marginTop: '2rem' }}>
            <button type="submit" className={styles.submitBtn} style={{ marginTop: 0 }}>
              Load Customer
            </button>
            <div style={{ textAlign: 'center', color: '#94a3b8', fontSize: '0.9rem' }}>OR</div>
            <label className={styles.secondaryBtn}>
              Upload CSV File
              <input type="file" accept=".csv" style={{ display: 'none' }} onChange={handleFileUpload} />
            </label>
          </div>
        </form>
      </div>
    </main>
  );
}
