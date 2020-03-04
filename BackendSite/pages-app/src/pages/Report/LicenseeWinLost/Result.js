import React, {useState} from "react";
import { AgGridReact } from 'ag-grid-react';
import { AllCommunityModules } from "@ag-grid-community/all-modules";
import "@ag-grid-community/all-modules/dist/styles/ag-grid.css";
import "@ag-grid-community/all-modules/dist/styles/ag-theme-blue.css";

if(process.env.NODE_ENV!=='production'){
    window.i18n = {"lbl_Currency2":"MYR","lbl_Currency4":"THB","lbl_Currency13":"RMB","lbl_Currency15":"IDR","lbl_Currency51":"VND"};
}

const Panel = (props)=>{
    const columnDefs = [
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
    
    const defaultColDef = {
        sortable: true,
        resizable: true,
        filter: true
    }

    let currency = props.SearchInfo.Currency == 0 ? 'all currencies' : window.i18n["lbl_Currency"+props.SearchInfo.Currency];
    
    const [stateData, setData] = useState();
    const onGridReady = ()=>{
        setData(props.Datas);
    }

    return(
        <div id="myGrid" className="ag-theme-blue table">
            <div className="table-options">
                <span className="text-bold">Licensee Win Lost from {props.SearchInfo.StartDate} to {props.SearchInfo.EndDate} in {currency}</span>
                {/* <button type="button" className="btn btn-excel">Export Excel</button> */}
            </div>
            <AgGridReact
                modules={AllCommunityModules}
                columnDefs={columnDefs}
                defaultColDef={defaultColDef}
                debug={false}
                rowData={stateData}
                onGridReady={onGridReady}
            />
        </div>
    )
}

export default Panel;