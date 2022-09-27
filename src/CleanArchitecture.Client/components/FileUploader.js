import React, { useState } from 'react';
import axios from 'axios';
import { useClient } from './ClientContext';

import { FilePond, registerPlugin } from 'react-filepond';
import 'filepond/dist/filepond.min.css';

import FilePondPluginMediaPreview from 'filepond-plugin-media-preview';
import 'filepond-plugin-media-preview/dist/filepond-plugin-media-preview.min.css';

import FilePondPluginImageExifOrientation from 'filepond-plugin-image-exif-orientation';

import FilePondPluginImagePreview from 'filepond-plugin-image-preview';
import 'filepond-plugin-image-preview/dist/filepond-plugin-image-preview.css';

import { AUTH_HEADER_NAME, AUTH_HEADER_PREFIX, SERVER_URL } from '../client';

// Register the plugins
registerPlugin(
    FilePondPluginImageExifOrientation,
    FilePondPluginImagePreview,
    FilePondPluginMediaPreview);

// Our app
export const FileUploader = (props) => {
    const [files, setFiles] = useState([]);
    const client = useClient();

    return (
        <FilePond
            files={files}
            onupdatefiles={setFiles}
            allowMultiple={true}
            allowVideoPreview={true}    // default true
            allowAudioPreview={true}
            maxFiles={3}
            name="files"
            labelIdle='Drag &amp; Drop your files or <span class="filepond--label-action">Browse</span>'
            chunkUploads={true}
            chunkForce={true}
            server={{
                url: SERVER_URL,
                process: {
                    url: '/media',
                    method: 'POST',
                    headers: (file) => {
                        const headers = {
                            'Upload-Name': file.name,
                            'Upload-Length': file.size,
                        };

                        if (client.user) { headers[AUTH_HEADER_NAME] = `${AUTH_HEADER_PREFIX}${client.user.accessToken}`; }
                        return headers;
                    },
                },
                patch: {
                    url: '',
                    headers: (chunk) => {
                        const headers = {
                            'Content-Type': 'application/offset+octet-stream',
                            'Upload-Offset': chunk.offset,
                            'Upload-Length': chunk.file.size,
                            'Upload-Name': chunk.file.name,
                        };

                        if (client.user) { headers[AUTH_HEADER_NAME] = `${AUTH_HEADER_PREFIX}${client.user.accessToken}`; }
                        return headers;
                    }
                },
                revert: (path, load, error) => {

                    // Helper Functions for Making XHR Requests in JavaScript
                    // source: https://gist.github.com/pizzarob/6c9efc583a17c2643505e7d70ffb1e0e
                    let xhr = new XMLHttpRequest();
                    xhr.open('DELETE', SERVER_URL + path);
                    xhr.setRequestHeader('Content-Type', 'application/offset+octet-stream');
                    if (client.user) { xhr.setRequestHeader(AUTH_HEADER_NAME, `${AUTH_HEADER_PREFIX}${client.user.accessToken}`); }

                    xhr.addEventListener('load', () => {
                        let { response, status } = xhr;
                        let res = () => { try { return JSON.parse(response); } catch (ex) { return response } };
                        if (status >= 200 && status < 400) {
                            load();
                        } else {
                            error(res);
                        }
                    });

                    xhr.send();
                },
            }} />
    )
}