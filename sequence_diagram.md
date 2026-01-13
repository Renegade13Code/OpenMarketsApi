``` mermaid
sequenceDiagram
    autonumber
    participant Client as API Client
    participant API as API Controller
    participant Service as UsersService
    participant UserRepo as UsersRepository
    participant PlanRepo as PlansRepository
    participant DB as Database

    Client->>+API: POST /api/users/{userId}/plans
    Note right of Client: Request Body: { "planId": 2 }
    
    API->>+Service: AddInsurancePlanToUserAsync(userId, planId)
    
    Service->>+PlanRepo: CheckInsurancePlanExists(planId)
    PlanRepo->>DB: Query plans table
    DB-->>PlanRepo: Return true (plan exists)
    PlanRepo-->>-Service: Return true
    
    Service->>+UserRepo: VerifyUserExists(userId)
    UserRepo->>DB: Query users table
    DB-->>UserRepo: Return true (user exists)
    UserRepo-->>-Service: Return true
    
    Service->>+UserRepo: AddInsurancePlanToUser(userId, planId)
    UserRepo->>DB: Insert into user_plan_selections table
    DB-->>UserRepo: Confirm insertion
    UserRepo-->>-Service: Return success
    
    Service-->>-API: Return success
    
    API-->>-Client: Return 201 Created
```