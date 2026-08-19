USE pizzatowndha;
-- =============================================
-- INSERT SAMPLE DATA
-- =============================================
INSERT INTO Units (
    Id,
    UnitSymbol,
    UnitName,
    Category,
    IsBaseUnit,
    ConversionFactor,
    DisplayOrder,
    CreatedBy,
    CreatedAt,
    UpdatedBy,
    UpdatedAt,
    IsDeleted
) VALUES
-- WEIGHT
('a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d', 'kg', 'Kilogram', 'Weight', 1, 1.0000000000, 1, 'SYSTEM', NOW(), NULL, NULL, 0),
('b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e', 'g', 'Gram', 'Weight', 0, 0.0010000000, 2, 'SYSTEM', NOW(), NULL, NULL, 0),
('c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'mg', 'Milligram', 'Weight', 0, 0.0000010000, 3, 'SYSTEM', NOW(), NULL, NULL, 0),
('d4e5f6a7-b8c9-4d0e-1f2a-3b4c5d6e7f8a', 'lb', 'Pound', 'Weight', 0, 0.4535923700, 4, 'SYSTEM', NOW(), NULL, NULL, 0),
('e5f6a7b8-c9d0-4e1f-2a3b-4c5d6e7f8a9b', 'oz', 'Ounce', 'Weight', 0, 0.0283495231, 5, 'SYSTEM', NOW(), NULL, NULL, 0),

-- VOLUME
('f6a7b8c9-d0e1-4f2a-3b4c-5d6e7f8a9b0c', 'L', 'Liter', 'Volume', 1, 1.0000000000, 6, 'SYSTEM', NOW(), NULL, NULL, 0),
('a7b8c9d0-e1f2-4a3b-4c5d-6e7f8a9b0c1d', 'mL', 'Milliliter', 'Volume', 0, 0.0010000000, 7, 'SYSTEM', NOW(), NULL, NULL, 0),
('b8c9d0e1-f2a3-4b4c-5d6e-7f8a9b0c1d2e', 'cup', 'Cup', 'Volume', 0, 0.2365882365, 8, 'SYSTEM', NOW(), NULL, NULL, 0),
('c9d0e1f2-a3b4-4c5d-6e7f-8a9b0c1d2e3f', 'tbsp', 'Tablespoon', 'Volume', 0, 0.0147867648, 9, 'SYSTEM', NOW(), NULL, NULL, 0),
('d0e1f2a3-b4c5-4d6e-7f8a-9b0c1d2e3f4a', 'tsp', 'Teaspoon', 'Volume', 0, 0.0049289216, 10, 'SYSTEM', NOW(), NULL, NULL, 0),

-- COUNT
('e1f2a3b4-c5d6-4e7f-8a9b-0c1d2e3f4a5b', 'pcs', 'Pieces', 'Count', 1, 1.0000000000, 11, 'SYSTEM', NOW(), NULL, NULL, 0),
('f2a3b4c5-d6e7-4f8a-9b0c-1d2e3f4a5b6c', 'doz', 'Dozen', 'Count', 0, 12.0000000000, 12, 'SYSTEM', NOW(), NULL, NULL, 0),
('a3b4c5d6-e7f8-4a9b-0c1d-2e3f4a5b6c7d', 'slice', 'Slice', 'Count', 0, 0.0000000000, 13, 'SYSTEM', NOW(), NULL, NULL, 0),
('b4c5d6e7-f8a9-4b0c-1d2e-3f4a5b6c7d8e', 'bottle', 'Bottle', 'Count', 0, 0.0000000000, 14, 'SYSTEM', NOW(), NULL, NULL, 0),
('c5d6e7f8-a9b0-4c1d-2e3f-4a5b6c7d8e9f', 'can', 'Can', 'Count', 0, 0.0000000000, 15, 'SYSTEM', NOW(), NULL, NULL, 0);