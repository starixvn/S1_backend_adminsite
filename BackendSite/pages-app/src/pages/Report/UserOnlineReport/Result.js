import React from "react";

let deviceListLength = 0;

const dataGroupColumnDefs = (props) => {
    deviceListLength = props.DeviceList.length;
    return [
        {
            columnTitle: 'Online Count',
            colSpan: (props.SiteList.length * (deviceListLength +1 )) + 1 ,
            attr:{
                class:"text-center",
            }
        }
       
    ]
}

const dataColumnDefs = (props) => {
    return [
        { 
            columnTitle: 'Site',
            dataColumn: 'site',
            body:{
                attr:{
                    class:"text-center",
                }
            }
        },
        ...props.SiteList.map(site=>{
            return({
                columnTitle: `${site.siteName}`,
                dataColumn: `online_${site.siteId}`,
                body:{
                    attr:{
                        class:"text-right",
                    }
                }
            })
        })
    ]
}

const dataDeviceColumnDefs = (props) => {
    let dateColResult=[];
    
    dateColResult.push({
        columnTitle: 'Local Time(GMT +8)',
        dataColumn: 'localTime',
        body:{
            attr:{
                class:"text-right",
            }
        }
    });
    
    props.SiteList.forEach((site, idx) => {
        
        props.DeviceList.forEach((element, idx) => {
            if(element.deviceType !== 0){
                dateColResult.push({
                    columnTitle: `${element.deviceName}`,
                    dataColumn: `online_${site.siteId}_${element.deviceType}`,
                    body:{
                        attr:{
                            class:"text-right",
                        }
                    }
                })
            }
        });
        props.DeviceList.forEach((element, idx) => {
            if(element.deviceType === 0){
                dateColResult.push({
                    columnTitle: `${element.deviceName}`,
                    dataColumn: `online_${site.siteId}_${element.deviceType}`,
                    body:{
                        attr:{
                            class:"text-right",
                        }
                    }
                })
            }
        });
        dateColResult.push({
            columnTitle: `Site Total`,
            dataColumn: `online_${site.siteId}_Summary`,
            body:{
                attr:{
                    class:"text-right",
                }
            }
        })
    }
    );

    dateColResult.push({
        columnTitle: 'Total Online',
        dataColumn: 'onlineSummary',
        body:{
            attr:{
                class:"text-right",
            }
        }
    });
    
    return dateColResult;
}

const Panel = (props) => {

    const groupColumnDefs = dataGroupColumnDefs(props);
    const columnDefs = dataColumnDefs(props);
    const deviceColumnDefs = dataDeviceColumnDefs(props);
    
    let groupHeaderRows = [];
    let headerRows = [];
    let deviceHeaderRows = [];
    
    groupColumnDefs.forEach((element, idx) => {
        groupHeaderRows.push(
            <th key={`th-${element.dataColumn}-${idx}`}
                scope="col"
                className={element && element.attr && element.attr.class ? element.attr.class : ''} 
                colSpan={element.colSpan}
                style={{position:"sticky", top:"0" }}>
                {element.columnTitle}
            </th>
        );
    });
    
    columnDefs.forEach((element, idx) => {
        if(idx > 0 ){
            element.colSpan = deviceListLength + 1;
        }
        headerRows.push(
            <th key={`th-${element.dataColumn}-${idx}`} 
                scope="col"
                colSpan={element.colSpan}
                className={element.header && element.header.attr && element.header.attr.class ? element.header.attr.class : ''}
                style={{position:"sticky", top:"0" }}>
                {element.columnTitle}
            </th>
        );
    });
    
    deviceColumnDefs.forEach((element, idx) => {
        deviceHeaderRows.push(
            <th key={`th-${element.dataColumn}`}
                scope="col"
                className={element && element.attr && element.attr.class ? element.attr.class : ''}
                colSpan={element.colSpan}
                style={{position:"sticky", top:"0" }}>
                {element.columnTitle}
            </th>
        );
    });
    
    let bodyRows = props.Datas.map((data, idx) => {
        let row = deviceColumnDefs.map((element, idx) => {
            let subData = {};
            if (element.body && element.body.subData) {
                element.body.subData.forEach(s => { subData[s] = data[s] });
            }
            return (
                <td key={`td-${element.dataColumn}-${idx}`} 
                    className={element.body && element.body.attr && element.body.attr.class ? element.body.attr.class : ''}
                    bgcolor={data.onlineSummary===props.MaxCount && props.MaxCount!==0 ? 'LemonChiffon' : ''}
                >
                    {element.body && element.body.format ? element.body.format(data[element.dataColumn], subData) : data[element.dataColumn]}
                </td>
            );
        });

        return (
            <tr key={`result-body-${idx}`}>
                {row}
            </tr>
        )
    });

    return (
        <div className="table">
            <div className="table-options">
                <button type="button" className="btn btn-approved" onClick={props.ExportBtnDisabled ? null : props.ExportToExcel} disabled={props.ExportBtnDisabled}>Export Excel</button>
            </div>
            <table className="table table-striped" style={{width:"auto"}}>
                <thead>
                    <tr>
                        <th style={{position:"sticky", top:"0" }}></th>
                        {groupHeaderRows}
                    </tr>
                    <tr>
                        {headerRows}
                        <th style={{position:"sticky", top:"0" }}></th>
                    </tr>
                    <tr>
                        {deviceHeaderRows}
                    </tr>
                </thead>
                <tbody>
                    {bodyRows}
                </tbody>
            </table>
        </div>
    )
}

export default Panel;