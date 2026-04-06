import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface CustomRole {
  id: number;
  accountId: number;
  name: string;
  description: string | null;
  permissions: string[];
  createdAt: string;
  updatedAt: string;
}

export interface CreateCustomRoleRequest {
  accountId: number;
  name: string;
  description?: string | null;
  permissions?: string[];
}

export interface UpdateCustomRoleRequest {
  name: string;
  description?: string | null;
  permissions?: string[];
}

export interface SamlConfig {
  id: number;
  accountId: number;
  idpEntityId: string;
  idpSsoTargetUrl: string;
  idpCertificate: string;
  spEntityId: string;
  assertionConsumerServiceUrl: string;
  nameIdentifierFormat: string | null;
  enabled: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface SamlRoleMapping {
  id: number;
  samlConfigId: number;
  samlAttributeValue: string;
  userRole: string;
}

export interface CreateSamlRoleMappingRequest {
  samlAttributeValue: string;
  userRole: string;
}

export interface AuditLog {
  id: number;
  accountId: number;
  userId: number;
  userName: string | null;
  action: string;
  auditableType: string;
  auditableId: number;
  changes: string | null;
  ipAddress: string | null;
  userAgent: string | null;
  createdAt: string;
}

export interface AuditLogFilter {
  userId?: number;
  action?: string;
  auditableType?: string;
  dateFrom?: string;
  dateTo?: string;
  page?: number;
  pageSize?: number;
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

@Injectable({
  providedIn: 'root',
})
export class EnterpriseService {
  private readonly api = inject(ApiService);

  // Custom Roles
  getCustomRoles(): Observable<CustomRole[]> {
    return this.api.get<CustomRole[]>(this.api.accountPath('/custom_roles'));
  }

  getCustomRole(id: number): Observable<CustomRole> {
    return this.api.get<CustomRole>(this.api.accountPath(`/custom_roles/${id}`));
  }

  createCustomRole(data: CreateCustomRoleRequest): Observable<CustomRole> {
    return this.api.post<CustomRole>(this.api.accountPath('/custom_roles'), data);
  }

  updateCustomRole(id: number, data: UpdateCustomRoleRequest): Observable<CustomRole> {
    return this.api.put<CustomRole>(this.api.accountPath(`/custom_roles/${id}`), data);
  }

  deleteCustomRole(id: number): Observable<void> {
    return this.api.delete<void>(this.api.accountPath(`/custom_roles/${id}`));
  }

  // SAML Config
  getSamlConfig(): Observable<SamlConfig> {
    return this.api.get<SamlConfig>(this.api.accountPath('/saml'));
  }

  createSamlConfig(data: Partial<SamlConfig>): Observable<SamlConfig> {
    return this.api.post<SamlConfig>(this.api.accountPath('/saml'), data);
  }

  updateSamlConfig(data: Partial<SamlConfig>): Observable<SamlConfig> {
    return this.api.put<SamlConfig>(this.api.accountPath('/saml'), data);
  }

  deleteSamlConfig(): Observable<void> {
    return this.api.delete<void>(this.api.accountPath('/saml'));
  }

  // SAML Role Mappings
  getSamlRoleMappings(): Observable<SamlRoleMapping[]> {
    return this.api.get<SamlRoleMapping[]>(this.api.accountPath('/saml/role_mappings'));
  }

  createSamlRoleMapping(data: CreateSamlRoleMappingRequest): Observable<SamlRoleMapping> {
    return this.api.post<SamlRoleMapping>(this.api.accountPath('/saml/role_mappings'), data);
  }

  deleteSamlRoleMapping(id: number): Observable<void> {
    return this.api.delete<void>(this.api.accountPath(`/saml/role_mappings/${id}`));
  }

  // Audit Logs
  getAuditLogs(filter: AuditLogFilter = {}): Observable<PaginatedResult<AuditLog>> {
    const params: Record<string, string | number | boolean> = {};
    if (filter.userId !== undefined) params['userId'] = filter.userId;
    if (filter.action) params['action'] = filter.action;
    if (filter.auditableType) params['auditableType'] = filter.auditableType;
    if (filter.dateFrom) params['dateFrom'] = filter.dateFrom;
    if (filter.dateTo) params['dateTo'] = filter.dateTo;
    params['page'] = filter.page ?? 1;
    params['pageSize'] = filter.pageSize ?? 25;
    return this.api.get<PaginatedResult<AuditLog>>(
      this.api.accountPath('/audit_logs'),
      params
    );
  }
}
