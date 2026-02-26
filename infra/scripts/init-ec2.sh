#!/bin/bash
# Initialize an Ubuntu 24.04/22.04 LTS EC2 instance for the Amagetdon backend

# 1. Update and install prerequisites
sudo apt-get update
sudo apt-get install -y ca-certificates curl gnupg lsb-release unzip

# 2. Add Docker's official GPG key
sudo mkdir -p /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg

# 3. Set up the Docker repository
echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
  $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

# 4. Install Docker Engine, containerd, and Docker Compose
sudo apt-get update
sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

# 5. Add the current user to the docker group (to run docker without sudo)
sudo usermod -aG docker $USER

# 6. Install AWS CLI
curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip"
unzip awscliv2.zip
sudo ./aws/install
rm -rf aws awscliv2.zip

echo "==================================="
echo "EC2 Initialization Complete."
echo "IMPORTANT: Please log out and log back in (or run 'newgrp docker') for docker group privileges to apply."
echo "==================================="
