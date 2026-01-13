const API_BASE_URL = 'http://localhost:8080';
const API_KEY = 'OM-PROD-8f9e2c7a-4b3d-4e8f-9c1a-2d5e6f7a8b9c';

interface TestResult {
  name: string;
  passed: boolean;
  message: string;
}

const results: TestResult[] = [];

async function runTest(name: string, testFn: () => Promise<void>): Promise<void> {
  try {
    await testFn();
    results.push({ name, passed: true, message: 'PASS' });
    console.log(`✓ ${name}`);
  } catch (error) {
    results.push({ name, passed: false, message: (error as Error).message });
    console.error(`✗ ${name}: ${(error as Error).message}`);
  }
}

async function testHealthEndpoint(): Promise<void> {
  const response = await fetch(`${API_BASE_URL}/health`);
  if (!response.ok) {
    throw new Error(`Expected 200, got ${response.status}`);
  }
}

async function testUnauthorizedRequest(): Promise<void> {
  const response = await fetch(`${API_BASE_URL}/api/companies/CBA`);
  if (response.status !== 401) {
    throw new Error(`Expected 401, got ${response.status}`);
  }
}

async function testInvalidApiKey(): Promise<void> {
  const response = await fetch(`${API_BASE_URL}/api/companies/CBA`, {
    headers: { 'X-Api-Key': 'invalid-key' }
  });
  if (response.status !== 401) {
    throw new Error(`Expected 401, got ${response.status}`);
  }
}

async function testValidCompanyLookup(): Promise<void> {
  const response = await fetch(`${API_BASE_URL}/api/companies/CBA`, {
    headers: { 'X-Api-Key': API_KEY }
  });
  if (!response.ok) {
    throw new Error(`Expected 200, got ${response.status}`);
  }
  const data = await response.json();
  if (!data.asxCode || !data.companyName || !data.gicsIndustry) {
    throw new Error('Response missing required fields');
  }
  if (data.asxCode !== 'CBA') {
    throw new Error(`Expected ASX code 'CBA', got '${data.asxCode}'`);
  }
}

async function testCaseInsensitiveLookup(): Promise<void> {
  const response = await fetch(`${API_BASE_URL}/api/companies/cba`, {
    headers: { 'X-Api-Key': API_KEY }
  });
  if (!response.ok) {
    throw new Error(`Expected 200, got ${response.status}`);
  }
  const data = await response.json();
  if (data.asxCode !== 'CBA') {
    throw new Error(`Expected ASX code 'CBA', got '${data.asxCode}'`);
  }
}

async function testInvalidCompanyCode(): Promise<void> {
  const response = await fetch(`${API_BASE_URL}/api/companies/INVALID123`, {
    headers: { 'X-Api-Key': API_KEY }
  });
  if (response.status !== 404) {
    throw new Error(`Expected 404, got ${response.status}`);
  }
  const data = await response.json();
  if (!data.message || !data.message.includes('not found')) {
    throw new Error('Expected error message about company not found');
  }
}

async function testEmptyCompanyCode(): Promise<void> {
  const response = await fetch(`${API_BASE_URL}/api/companies/`, {
    headers: { 'X-Api-Key': API_KEY }
  });
  if (response.status !== 404 && response.status !== 405) {
    throw new Error(`Expected 404 or 405, got ${response.status}`);
  }
}

async function testCacheHit(): Promise<void> {
  const code = 'BHP';

  const response1 = await fetch(`${API_BASE_URL}/api/companies/${code}`, {
    headers: { 'X-Api-Key': API_KEY }
  });
  if (!response1.ok) {
    throw new Error(`First request failed: ${response1.status}`);
  }

  const response2 = await fetch(`${API_BASE_URL}/api/companies/${code}`, {
    headers: { 'X-Api-Key': API_KEY }
  });
  if (!response2.ok) {
    throw new Error(`Second request failed: ${response2.status}`);
  }

  const data1 = await response1.json();
  const data2 = await response2.json();

  if (JSON.stringify(data1) !== JSON.stringify(data2)) {
    throw new Error('Cached response differs from original');
  }
}

async function testMultipleCompanies(): Promise<void> {
  const companies = ['CBA', 'BHP', 'WBC', 'ANZ', 'NAB'];

  for (const code of companies) {
    const response = await fetch(`${API_BASE_URL}/api/companies/${code}`, {
      headers: { 'X-Api-Key': API_KEY }
    });
    if (!response.ok) {
      throw new Error(`Failed to fetch ${code}: ${response.status}`);
    }
    const data = await response.json();
    if (data.asxCode !== code) {
      throw new Error(`Expected ${code}, got ${data.asxCode}`);
    }
  }
}

async function runAllTests(): Promise<void> {
  console.log('Starting integration tests...\n');

  await runTest('Health endpoint returns 200', testHealthEndpoint);
  await runTest('Unauthorized request returns 401', testUnauthorizedRequest);
  await runTest('Invalid API key returns 401', testInvalidApiKey);
  await runTest('Valid company lookup returns 200', testValidCompanyLookup);
  await runTest('Case-insensitive lookup works', testCaseInsensitiveLookup);
  await runTest('Invalid company code returns 404', testInvalidCompanyCode);
  await runTest('Empty company code returns 404/405', testEmptyCompanyCode);
  await runTest('Cache returns consistent data', testCacheHit);
  await runTest('Multiple company lookups succeed', testMultipleCompanies);

  console.log('\n=== Test Summary ===');
  const passed = results.filter(r => r.passed).length;
  const failed = results.filter(r => !r.passed).length;
  console.log(`Passed: ${passed}`);
  console.log(`Failed: ${failed}`);
  console.log(`Total: ${results.length}`);

  if (failed > 0) {
    console.log('\nFailed tests:');
    results.filter(r => !r.passed).forEach(r => {
      console.log(`  - ${r.name}: ${r.message}`);
    });
    process.exit(1);
  } else {
    console.log('\n✓ All tests passed!');
    process.exit(0);
  }
}

runAllTests().catch(error => {
  console.error('Test suite failed:', error);
  process.exit(1);
});
