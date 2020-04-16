import React, {useState} from "react";
import { AgGridReact } from 'ag-grid-react';
import { AllCommunityModules } from "@ag-grid-community/all-modules";
import "@ag-grid-community/all-modules/dist/styles/ag-grid.css";
import "@ag-grid-community/all-modules/dist/styles/ag-theme-blue.css";

if(process.env.NODE_ENV!=='production'){
    window.i18n = {"lbl_Currency2":"MYR","lbl_Currency4":"THB","lbl_Currency13":"RMB","lbl_Currency15":"IDR","lbl_Currency51":"VND"};
}

const defaultColDef = {
    sortable: true,
    resizable: true,
    filter: true
}

const columnByDateDefs = (props)=>{
    return [
        {
            headerName: "Date",
            field: "date",
            width: 100,
        },
        {
            headerName: "Currency",
            field: "currencyName",
            width: 100,
        },
        {
            headerName: "Turnover Amount",
            width: 200,
            children: [
                {
                    headerName: "Total",
                    field: "stakeSummary",
                    width: 200,
                    columnGroupShow: "closed",
                },
                ...props.GameList.map(game=>{
                    return(
                        {
                            headerName: game.name,
                            columnGroupShow: "open",
                            field: `stake_game_${game.type}`,
                            width: 200
                        }
                    )
                })
            ]
        },
        {
            headerName: "WinLost Amount",
            width: 200,
            children: [
                {
                    headerName: "Total",
                    field: "winlostAmount",
                    width: 200,
                    columnGroupShow: "closed",
                },
                ...props.GameList.map(game=>{
                    return(
                        {
                            headerName: game.name,
                            columnGroupShow: "open",
                            field: `winlost_game_${game.type}`,
                            width: 200
                        }
                    )
                })
            ]
        }
    ];
}

const columnTotalDefs = (props)=>{
    return [
        {
            headerName: "Currency",
            field: "currencyName",
            width: 100,
        },
        {
            headerName: "Turnover Amount",
            width: 200,
            children: [
                {
                    headerName: "Total",
                    field: "stakeSummary",
                    width: 200,
                    columnGroupShow: "closed",
                },
                ...props.GameList.map(game=>{
                    return(
                        {
                            headerName: game.name,
                            columnGroupShow: "open",
                            field: `stake_game_${game.type}`,
                            width: 200
                        }
                    )
                })
            ]
        },
        {
            headerName: "WinLost Amount",
            width: 200,
            children: [
                {
                    headerName: "Total",
                    field: "winlostAmount",
                    width: 200,
                    columnGroupShow: "closed",
                },
                ...props.GameList.map(game=>{
                    return(
                        {
                            headerName: game.name,
                            columnGroupShow: "open",
                            field: `winlost_game_${game.type}`,
                            width: 200
                        }
                    )
                })
            ]
        }
    ];
}

const Panel = (props)=>{
    let currency = props.SearchInfo.Currency == 0 ? 'all currencies' : window.i18n["lbl_Currency"+props.SearchInfo.Currency];
    
    const [stateData, setData] = useState({
            Datas:props.Datas,
            TotalDatas:props.TotalDatas
        });
    const onGridReady = ()=>{
        setData({
            Datas:props.Datas,
            TotalDatas:props.TotalDatas
        });
    }

    return(
        <div id="myGrid" className="ag-theme-blue table">
            <div className="table-options">
                <span className="text-bold">Win Lost from {props.SearchInfo.StartDate} to {props.SearchInfo.EndDate} in {currency}</span>
                <button type="button" className="btn btn-excel" onClick={props.ExportToExcel}>Export Excel</button>
            </div>
            <AgGridReact
                modules={AllCommunityModules}
                columnDefs={columnByDateDefs(props)}
                defaultColDef={defaultColDef}
                debug={false}
                rowData={stateData.Datas}
                onGridReady={onGridReady}
            />
            <div style={{ height: '50%' }} className="ag-theme-balham">
                <AgGridReact
                    modules={AllCommunityModules}
                    columnDefs={columnTotalDefs(props)}
                    defaultColDef={defaultColDef}
                    debug={false}
                    rowData={stateData.TotalDatas}
                    onGridReady={onGridReady}
                />
            </div>
        </div>
    )
}

export default Panel;