#!/usr/bin/env python3
"""
MCP Chat Screenshot Capture Script

This script uses Playwright to automatically capture screenshots of the MCP Chat feature.
It navigates through the application, performs operations, and captures screenshots at key points.

Prerequisites:
1. Install Playwright: pip install playwright
2. Install browsers: playwright install chromium
3. Aspire AppHost running: dotnet run --project src/AppHost
4. Get the BlazorWeb URL from Aspire dashboard

Usage:
    python3 .github/scripts/capture-mcp-screenshots.py <blazorweb-url>
    
Example:
    python3 .github/scripts/capture-mcp-screenshots.py http://localhost:5001
"""

import asyncio
import sys
import os
from pathlib import Path

try:
    from playwright.async_api import async_playwright, TimeoutError as PlaywrightTimeoutError
except ImportError:
    print("Error: Playwright is not installed.")
    print("Install it with: pip install playwright")
    print("Then install browsers with: playwright install chromium")
    sys.exit(1)


async def capture_screenshots(base_url: str, output_dir: str):
    """Capture screenshots of MCP Chat functionality."""
    
    # Create output directory
    output_path = Path(output_dir)
    output_path.mkdir(parents=True, exist_ok=True)
    
    async with async_playwright() as p:
        # Launch browser
        browser = await p.chromium.launch(headless=True)
        context = await browser.new_context(
            viewport={'width': 1920, 'height': 1080},
            locale='ja-JP'
        )
        page = await context.new_page()
        
        print(f"Navigating to {base_url}")
        
        try:
            # Screenshot 1: Home page / Login
            print("1. Capturing home page...")
            await page.goto(base_url, wait_until='networkidle', timeout=30000)
            await page.screenshot(path=str(output_path / "01-home-page.png"))
            
            # Check if login is required
            if "login" in page.url.lower():
                print("   Login page detected, capturing login screen...")
                await page.screenshot(path=str(output_path / "01-login-page.png"))
                # Note: Actual login would require credentials
                print("   Note: Manual login may be required. Continuing...")
            
            # Screenshot 2: Navigate to MCP Chat
            print("2. Navigating to MCP Chat...")
            try:
                # Try to find MCP Chat link in navigation
                await page.click('text=MCPチャット', timeout=5000)
                await page.wait_for_load_state('networkidle', timeout=10000)
            except PlaywrightTimeoutError:
                # If not found, navigate directly
                await page.goto(f"{base_url}/mcp-chat", wait_until='networkidle')
            
            await page.screenshot(path=str(output_path / "02-mcp-chat-initial.png"), full_page=True)
            
            # Screenshot 3: Before connection
            print("3. Capturing initial connection screen...")
            await page.wait_for_selector('text=MCPサーバーに接続', timeout=5000)
            await page.screenshot(path=str(output_path / "03-before-connection.png"), full_page=True)
            
            # Screenshot 4: Click connect button
            print("4. Clicking connect button...")
            connect_button = await page.query_selector('button:has-text("全サービスに接続")')
            if connect_button:
                await connect_button.click()
                # Wait a bit for connection
                await page.wait_for_timeout(3000)
                await page.screenshot(path=str(output_path / "04-connecting.png"), full_page=True)
                
                # Wait for connection to complete
                await page.wait_for_timeout(5000)
                
                # Screenshot 5: After connection
                print("5. Capturing connected state...")
                await page.screenshot(path=str(output_path / "05-connected.png"), full_page=True)
                
                # Screenshot 6: Select Employee Service
                print("6. Selecting Employee Service...")
                try:
                    employee_service = await page.query_selector('text=従業員サービス')
                    if employee_service:
                        await employee_service.click()
                        await page.wait_for_timeout(1000)
                        await page.screenshot(path=str(output_path / "06-employee-service-selected.png"), full_page=True)
                except Exception as e:
                    print(f"   Warning: Could not select Employee Service: {e}")
                
                # Screenshot 7: Tool list
                print("7. Capturing tool list...")
                await page.screenshot(path=str(output_path / "07-tool-list.png"), full_page=True)
                
                # Screenshot 8: Select and execute a tool
                print("8. Attempting to execute ListEmployeesAsync...")
                try:
                    list_employees = await page.query_selector('text=ListEmployeesAsync')
                    if list_employees:
                        await list_employees.click()
                        await page.wait_for_timeout(1000)
                        
                        # Execute the tool
                        execute_button = await page.query_selector('button:has-text("ツールを実行")')
                        if execute_button:
                            await execute_button.click()
                            await page.wait_for_timeout(3000)
                            await page.screenshot(path=str(output_path / "08-tool-execution.png"), full_page=True)
                            
                            # Screenshot 9: Results in chat
                            print("9. Capturing chat results...")
                            await page.wait_for_timeout(2000)
                            await page.screenshot(path=str(output_path / "09-chat-results.png"), full_page=True)
                except Exception as e:
                    print(f"   Warning: Could not execute tool: {e}")
                
                # Screenshot 10: Help guide
                print("10. Opening help guide...")
                try:
                    help_panel = await page.query_selector('text=使い方ガイド')
                    if help_panel:
                        await help_panel.click()
                        await page.wait_for_timeout(1000)
                        await page.screenshot(path=str(output_path / "10-help-guide.png"), full_page=True)
                except Exception as e:
                    print(f"   Warning: Could not open help guide: {e}")
            
            print(f"\n✅ Screenshots captured successfully in: {output_path}")
            print(f"   Total screenshots: {len(list(output_path.glob('*.png')))}")
            
        except Exception as e:
            print(f"❌ Error during screenshot capture: {e}")
            # Capture error state
            await page.screenshot(path=str(output_path / "error-state.png"), full_page=True)
            raise
        
        finally:
            await browser.close()


def main():
    """Main entry point."""
    if len(sys.argv) < 2:
        print("Usage: python3 capture-mcp-screenshots.py <blazorweb-url>")
        print("Example: python3 capture-mcp-screenshots.py http://localhost:5001")
        sys.exit(1)
    
    base_url = sys.argv[1].rstrip('/')
    output_dir = ".github/issue-reports/screenshots"
    
    print("=" * 60)
    print("MCP Chat Screenshot Capture")
    print("=" * 60)
    print(f"Base URL: {base_url}")
    print(f"Output Directory: {output_dir}")
    print("=" * 60)
    print()
    
    # Check if Playwright browsers are installed
    print("Checking Playwright installation...")
    try:
        asyncio.run(capture_screenshots(base_url, output_dir))
    except Exception as e:
        print(f"\n❌ Failed to capture screenshots: {e}")
        print("\nTroubleshooting:")
        print("1. Make sure Aspire AppHost is running: dotnet run --project src/AppHost")
        print("2. Verify BlazorWeb URL is accessible")
        print("3. Check if Playwright browsers are installed: playwright install chromium")
        sys.exit(1)


if __name__ == "__main__":
    main()
