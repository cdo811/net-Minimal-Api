'use client';
import { useState } from 'react';
import styles from './page.module.css';

export default function CustomerForm() {
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

  const sendLog = async (level: string, message: string, meta: any = {}) => {
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

  return (
    <main className={styles.main}>
      <div className={styles.formContainer}>
        <h1 className={styles.title}>Load Customer</h1>
        <p className={styles.subtitle}>Enter the individual customer details below</p>
        
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
          
          <button type="submit" className={styles.submitBtn}>
            Load Customer
          </button>
        </form>
      </div>
    </main>
  );
}
